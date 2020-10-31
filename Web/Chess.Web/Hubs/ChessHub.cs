namespace Chess.Web.Hubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Chess.Common.Enums;
    using Chess.Data.Models;
    using Chess.Data.Models.EventArgs;

    using Microsoft.AspNetCore.SignalR;

    public class ChessHub : Hub
    {
        private readonly ConcurrentDictionary<string, Player> players;
        private readonly ConcurrentDictionary<string, Game> games;
        private readonly ConcurrentQueue<Player> waitingPlayers;

        public ChessHub()
        {
            this.players = new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);
            this.games = new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);
            this.waitingPlayers = new ConcurrentQueue<Player>();
        }

        public async Task FindGame(string name)
        {
            Player joiningPlayer = Factory.GetPlayer(name, this.Context.ConnectionId);
            this.players[joiningPlayer.Id] = joiningPlayer;
            await this.Clients.Caller.SendAsync("PlayerJoined", joiningPlayer);

            this.waitingPlayers.TryDequeue(out Player opponent);

            if (opponent == null)
            {
                this.waitingPlayers.Enqueue(joiningPlayer);
                await this.Clients.Caller.SendAsync("WaitingList");
            }
            else
            {
                await this.StartGame(joiningPlayer, opponent);
            }
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
            }
            else
            {
                await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("DrawOfferRejected", player);
            }
        }

        public async Task SendMessage(string message)
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];
            var dt = DateTime.Now;

            await this.Clients.Group(game.Id).SendAsync("UpdateChat", message, player, dt.ToString("HH:mm"));
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var leavingPlayer = this.players[this.Context.ConnectionId];
            this.Clients.Group(leavingPlayer.GameId).SendAsync("GameOver", leavingPlayer, GameOver.Disconnected);

            return base.OnDisconnectedAsync(exception);
        }

        private async Task StartGame(Player joiningPlayer, Player opponent)
        {
            joiningPlayer.Color = Color.Black;
            opponent.Color = Color.White;
            opponent.HasToMove = true;

            var game = Factory.GetGame(opponent, joiningPlayer);
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
        }

        private void Game_OnGameOver(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var gameOver = e as GameOverEventArgs;

            this.Clients.Group(game.Id).SendAsync("GameOver", player, gameOver.GameOver);
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
