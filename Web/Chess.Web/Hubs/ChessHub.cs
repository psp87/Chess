namespace Chess.Web.Hubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Chess.Common.Enums;
    using Chess.Data.Models;
    using Chess.Data.Models.EventArgs;

    using Microsoft.AspNetCore.SignalR;

    public class ChessHub : Hub
    {
        private readonly ConcurrentDictionary<string, Player> players;
        private readonly ConcurrentDictionary<string, Game> games;
        private readonly List<Player> waitingPlayers;

        public ChessHub()
        {
            this.players = new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);
            this.games = new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);
            this.waitingPlayers = new List<Player>();
        }

        public override Task OnConnectedAsync()
        {
            this.Clients.Caller.SendAsync("ListRooms", this.waitingPlayers);

            var msgFormat = $"{this.Context.User.Identity.Name} joined the lobby";
            this.Clients.All.SendAsync("UpdateLobbyChatInternalMessage", msgFormat);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var leavingPlayer = this.players[this.Context.ConnectionId];
            if (leavingPlayer.GameId != null)
            {
                this.Clients.Group(leavingPlayer.GameId).SendAsync("GameOver", leavingPlayer, GameOver.Disconnected);

                var msgFormat = $"{leavingPlayer.Name} has left. You won!";
                this.Clients.Group(leavingPlayer.GameId).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
            }

            this.waitingPlayers.Remove(leavingPlayer);
            this.Clients.All.SendAsync("ListRooms", this.waitingPlayers);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task CreateRoom(string name)
        {
            Player player = Factory.GetPlayer(name, this.Context.ConnectionId);
            this.players[player.Id] = player;
            this.waitingPlayers.Add(player);
            await this.Clients.Caller.SendAsync("PlayerJoined", player);
            await this.Clients.Caller.SendAsync("EnterRoom", name);
            await this.Clients.All.SendAsync("AddRoom", player);
        }

        public async Task JoinRoom(string name, string id)
        {
            Player joiningPlayer = Factory.GetPlayer(name, this.Context.ConnectionId);
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

            if (player.IsThreefoldDrawAvailable && player.HasToMove)
            {
                await this.Clients.Group(game.Id).SendAsync("GameOver", player, GameOver.ThreefoldDraw);
            }
        }

        public async Task Resign()
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];

            await this.Clients.Group(game.Id).SendAsync("GameOver", player, GameOver.Resign);

            var msgFormat = $"{player.Name} resigned!";
            await this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
        }

        public async Task OfferDrawRequest()
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];

            await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("DrawOffered", player);
        }

        public async Task OfferDrawAnswer(bool isAccepted)
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];

            if (isAccepted)
            {
                await this.Clients.Group(game.Id).SendAsync("GameOver", null, GameOver.Draw);

                var msgFormat = $"{player.Name} accepted the offer. Draw!";
                await this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
            }
            else
            {
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
            var gameOver = e as GameOverEventArgs;

            this.Clients.Group(game.Id).SendAsync("GameOver", player, gameOver.GameOver);

            var msgFormat = $"{player.Name} wins by {gameOver.GameOver.ToString()}";
            this.Clients.Group(game.Id).SendAsync("UpdateGameChatInternalMeesage", msgFormat);
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
