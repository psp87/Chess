namespace Chess.Web.Hubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Chess.Common.Enums;
    using Chess.Data;
    using Chess.Data.Models;
    using Chess.Data.Models.EventArgs;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;

    public class ChessHub : Hub
    {
        private readonly ConcurrentDictionary<string, Player> players;
        private readonly ConcurrentDictionary<string, Game> games;
        private readonly List<Player> waitingPlayers;
        private readonly IServiceProvider sp;

        public ChessHub(IServiceProvider sp)
        {
            this.players = new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);
            this.games = new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);
            this.waitingPlayers = new List<Player>();
            this.sp = sp;
        }

        public override Task OnConnectedAsync()
        {
            var msgFormat = $"{this.Context.User.Identity.Name} joined the lobby";
            this.Clients.All.SendAsync("UpdateLobbyChatInternalMessage", msgFormat);
            this.Clients.Caller.SendAsync("ListRooms", this.waitingPlayers);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var leavingPlayer = this.players[this.Context.ConnectionId];
            var game = this.games[leavingPlayer.GameId];
            var winner = game.Opponent;

            if (leavingPlayer.GameId != null)
            {
                var msgFormat = $"{leavingPlayer.Name} left. You won!";
                this.Clients.Group(leavingPlayer.GameId).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
                this.Clients.Group(leavingPlayer.GameId).SendAsync("GameOver", leavingPlayer, GameOver.Disconnected);

                this.UpdateStats(winner, leavingPlayer, game, GameOver.Disconnected);
            }

            this.waitingPlayers.Remove(leavingPlayer);
            this.Clients.All.SendAsync("ListRooms", this.waitingPlayers);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task CreateRoom(string name)
        {
            Player player = Factory.GetPlayer(name, this.Context.ConnectionId, this.Context.UserIdentifier);
            this.players[player.Id] = player;
            this.waitingPlayers.Add(player);

            var msgFormat = $"{player.Name} created a room";
            await this.Clients.All.SendAsync("UpdateLobbyChatInternalMessage", msgFormat);
            await this.Clients.Caller.SendAsync("EnterRoom", name);
            await this.Clients.Caller.SendAsync("PlayerJoined", player);
            await this.Clients.All.SendAsync("AddRoom", player);
        }

        public async Task JoinRoom(string name, string id)
        {
            Player joiningPlayer = Factory.GetPlayer(name, this.Context.ConnectionId, this.Context.UserIdentifier);
            this.players[joiningPlayer.Id] = joiningPlayer;
            await this.Clients.Caller.SendAsync("PlayerJoined", joiningPlayer);
            var opponent = this.players[id];
            await this.StartGame(opponent, joiningPlayer);
        }

        public async Task LobbySendMessage(string message)
        {
            var msgFormat = $"{DateTime.Now.ToString("HH:mm")}, {this.Context.User.Identity.Name}: {message}";
            await this.Clients.All.SendAsync("UpdateLobbyChat", msgFormat);
        }

        public async Task MoveSelected(string source, string target, string sourceFen, string targetFen)
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];

            if (!player.HasToMove ||
                !game.MakeMove(source, target, targetFen))
            {
                await this.Clients.Caller.SendAsync("BoardSnapback", sourceFen);
                return;
            }

            await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("BoardMove", source, target);
            await this.Clients.Group(game.Id).SendAsync("HighlightMove", source, target, game.Opponent);

            if (game.GameOver.ToString() == GameOver.None.ToString())
            {
                await this.Clients.Group(game.Id).SendAsync("UpdateStatus", game.MovingPlayer.Name);
            }

            if (game.Move.Type != MoveType.Normal)
            {
                switch (game.Move.Type)
                {
                    case MoveType.Castling:
                        await this.Clients.Group(game.Id).SendAsync("BoardMove", game.Move.CastlingArgs.RookSource, game.Move.CastlingArgs.RookTarget);
                        break;
                    case MoveType.EnPassant:
                        await this.Clients.Group(game.Id).SendAsync("EnPassantTake", game.Move.EnPassantArgs.SquareTakenPiece.Name, target);
                        break;
                    case MoveType.PawnPromotion:
                        await this.Clients.Group(game.Id).SendAsync("BoardSetPosition", game.Move.PawnPromotionArgs.FenString);
                        break;
                }

                game.Move.Type = MoveType.Normal;
            }
        }

        public async Task IsThreefoldDraw()
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];
            var opponent = game.Opponent;

            if (player.IsThreefoldDrawAvailable && player.HasToMove)
            {
                var msgFormat = $"{player.Name} declared threefold draw!";
                await this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
                await this.Clients.Group(game.Id).SendAsync("GameOver", player, GameOver.ThreefoldDraw);

                this.UpdateStats(player, opponent, game, GameOver.Draw);
            }
        }

        public async Task Resign()
        {
            var loser = this.players[this.Context.ConnectionId];
            var game = this.games[loser.GameId];
            var winner = game.Opponent;

            var msgFormat = $"{loser.Name} resigned!";
            await this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
            await this.Clients.Group(game.Id).SendAsync("GameOver", loser, GameOver.Resign);

            this.UpdateStats(winner, loser, game, GameOver.Resign);
        }

        public async Task OfferDrawRequest()
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];

            var msgFormat = $"{player.Name} requested a draw!";
            await this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
            await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("DrawOffered", player);
        }

        public async Task OfferDrawAnswer(bool isAccepted)
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];
            var opponent = game.Opponent;

            if (isAccepted)
            {
                var msgFormat = $"{player.Name} accepted the offer. Draw!";
                await this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
                await this.Clients.Group(game.Id).SendAsync("GameOver", null, GameOver.Draw);

                this.UpdateStats(player, opponent, game, GameOver.Draw);
            }
            else
            {
                var msgFormat = $"{player.Name} rejected the offer!";
                await this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
                await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("DrawOfferRejected", player);
            }
        }

        public async Task GameSendMessage(string message)
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];

            var msgFormat = $"{DateTime.Now.ToString("HH:mm")}, {player.Name}: {message}";
            await this.Clients.Group(game.Id).SendAsync("UpdateGameChat", msgFormat, player);
        }

        private async Task StartGame(Player player1, Player player2)
        {
            player1.Color = Color.White;
            player2.Color = Color.Black;
            player1.HasToMove = true;

            var game = Factory.GetGame(player1, player2);
            this.games[game.Id] = game;

            game.OnGameOver += this.Game_OnGameOver;
            game.OnMoveComplete += this.Game_OnMoveComplete;
            game.OnMoveEvent += this.Game_OnMoveEvent;
            game.OnTakePiece += this.Game_OnTakePiece;
            game.OnThreefoldDrawAvailable += this.Game_OnThreefoldDrawAvailable;

            await Task.WhenAll(
                this.Groups.AddToGroupAsync(game.Player1.Id, groupName: game.Id),
                this.Groups.AddToGroupAsync(game.Player2.Id, groupName: game.Id),
                this.Clients.Group(game.Id).SendAsync("Start", game));

            await this.Clients.Caller.SendAsync("ChangeOrientation");

            this.waitingPlayers.Remove(player1);
            await this.Clients.All.SendAsync("ListRooms", this.waitingPlayers);

            var msgFormat = $"{player2.Name} joined. The game started!";
            await this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
        }

        private void Game_OnGameOver(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var opponent = game.Opponent;
            var gameOver = e as GameOverEventArgs;

            var msgFormat = $"{gameOver.GameOver.ToString()}!";
            this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMessage", msgFormat);
            this.Clients.Group(game.Id).SendAsync("GameOver", player, gameOver.GameOver);

            this.UpdateStats(player, opponent, game, gameOver.GameOver);
        }

        private void UpdateStats(Player sender, Player opponent, Game game, GameOver gameOver)
        {
            using (var scope = this.sp.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var senderStats = dbContext.Stats.Where(x => x.Owner.Id == sender.UserId).FirstOrDefault();
                var opponentStats = dbContext.Stats.Where(x => x.Owner.Id == opponent.UserId).FirstOrDefault();

                if (senderStats == null)
                {
                    senderStats = new Stats
                    {
                        Games = 0,
                        Wins = 0,
                        Draws = 0,
                        Losses = 0,
                        Rating = 1200,
                        OwnerId = sender.UserId,
                    };

                    dbContext.Stats.Add(senderStats);
                    dbContext.SaveChanges();
                }

                if (opponentStats == null)
                {
                    opponentStats = new Stats
                    {
                        Games = 0,
                        Wins = 0,
                        Draws = 0,
                        Losses = 0,
                        Rating = 1200,
                        OwnerId = opponent.UserId,
                    };

                    dbContext.Stats.Add(opponentStats);
                    dbContext.SaveChanges();
                }

                senderStats.Games += 1;
                opponentStats.Games += 1;

                if (gameOver == GameOver.Checkmate || gameOver == GameOver.Resign || gameOver == GameOver.Disconnected)
                {
                    int points = game.CalculateRatingPoints(senderStats.Rating, opponentStats.Rating);

                    senderStats.Wins += 1;
                    senderStats.Rating += points;

                    opponentStats.Rating -= points;
                    opponentStats.Losses += 1;
                }
                else if (gameOver == GameOver.Stalemate || gameOver == GameOver.Draw || gameOver == GameOver.ThreefoldDraw || gameOver == GameOver.FivefoldDraw)
                {
                    senderStats.Draws += 1;
                    opponentStats.Draws += 1;
                }

                dbContext.Stats.Update(senderStats);
                dbContext.Stats.Update(opponentStats);
                dbContext.SaveChanges();
            }
        }

        private void Game_OnMoveComplete(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var args = e as MoveCompleteEventArgs;

            this.Clients.Group(game.Id).SendAsync("UpdateMoveHistory", player, args.Notation);
        }

        private void Game_OnMoveEvent(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var message = e as MoveEventArgs;

            if (message.Type == Message.CheckOpponent || message.Type == Message.CheckClear)
            {
                if (message.Type == Message.CheckOpponent)
                {
                    var msgFormat = $"{player.Name} checked the opponent!";
                    this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
                }

                this.Clients.Group(game.Id).SendAsync("CheckStatus", message.Type);
            }
            else
            {
                this.Clients.Caller.SendAsync("InvalidMove", message.Type);
            }
        }

        private void Game_OnTakePiece(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var args = e as TakePieceEventArgs;

            this.Clients.Group(game.Id).SendAsync("UpdateTakenFigures", player, args.PieceName, args.Points);
        }

        private void Game_OnThreefoldDrawAvailable(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var args = e as ThreefoldDrawEventArgs;

            this.Clients.Caller.SendAsync("ThreefoldAvailable", player, false);
            this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("ThreefoldAvailable", player, args.IsAvailable);
        }
    }
}
