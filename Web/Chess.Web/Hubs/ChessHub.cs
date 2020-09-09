namespace Chess.Web.Hubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Chess.Common;
    using Chess.Common.Enums;
    using Chess.Data.Models;
    using Chess.Data.Models.EventArgs;
    using Chess.Data.Models.Pieces.Helpers;

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

            if (GlobalConstants.GameOver.ToString() == GameOver.None.ToString())
            {
                await this.Clients.Group(game.Id).SendAsync("UpdateStatus", game.MovingPlayer.Name);
            }

            if (GlobalConstants.EnPassantTake != null)
            {
                await this.Clients.Group(game.Id).SendAsync("EnPassantTake", GlobalConstants.EnPassantTake, target);
                GlobalConstants.EnPassantTake = null;
            }

            if (GlobalConstants.CastlingMove)
            {
                await this.Clients.Group(game.Id).SendAsync("BoardMove", Castling.RookSource, Castling.RookTarget);
                GlobalConstants.CastlingMove = false;
            }

            if (GlobalConstants.PawnPromotionFen != null)
            {
                await this.Clients.Group(game.Id).SendAsync("BoardSetPosition", GlobalConstants.PawnPromotionFen);
                GlobalConstants.PawnPromotionFen = null;
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

        private async Task StartGame(Player joiningPlayer, Player opponent)
        {
            joiningPlayer.Color = Color.Black;
            opponent.Color = Color.White;
            opponent.HasToMove = true;

            var game = Factory.GetGame(opponent, joiningPlayer);
            this.games[game.Id] = game;

            game.ChessBoard.OnMoveComplete += this.Board_OnMoveComplete;
            game.OnGameOver += this.Game_OnGameOver;
            game.ChessBoard.OnMessage += this.Game_OnNotification;
            game.OnNotification += this.Game_OnNotification;
            game.ChessBoard.OnTakePiece += this.Board_OnTakePiece;
            game.ChessBoard.OnThreefoldDrawAvailable += this.Board_OnThreefoldDrawAvailable;

            await Task.WhenAll(
                this.Groups.AddToGroupAsync(game.Player1.Id, groupName: game.Id),
                this.Groups.AddToGroupAsync(game.Player2.Id, groupName: game.Id),
                this.Clients.Group(game.Id).SendAsync("Start", game));

            await this.Clients.Caller.SendAsync("ChangeOrientation");
        }

        private void Board_OnThreefoldDrawAvailable(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var args = e as ThreefoldDrawEventArgs;

            this.Clients.Caller.SendAsync("ThreefoldAvailable", player, false);
            this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("ThreefoldAvailable", player, args.IsAvailable);
        }

        private void Game_OnGameOver(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var gameOver = e as GameOverEventArgs;

            this.Clients.Group(game.Id).SendAsync("GameOver", player, gameOver.GameOver);
        }

        private void Board_OnMoveComplete(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var args = e as NotationEventArgs;

            this.Clients.Group(game.Id).SendAsync("UpdateMoveHistory", player, args.Notation);
        }

        private void Game_OnNotification(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var notification = e as MessageEventArgs;

            switch (notification.Type)
            {
                case Notification.InvalidMove:
                    this.Clients.Caller.SendAsync("InvalidMessage");
                    break;
                case Notification.CheckClear:
                    this.Clients.Group(game.Id).SendAsync("EmptyCheckStatus");
                    break;
                case Notification.CheckOpponent:
                    this.Clients.Group(game.Id).SendAsync("CheckOpponent");
                    break;
                case Notification.CheckSelf:
                    this.Clients.Caller.SendAsync("CheckSelf");
                    break;
            }
        }

        private void Board_OnTakePiece(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var args = e as TakePieceEventArgs;

            this.Clients.Group(game.Id).SendAsync("UpdateTakenFigures", player, args.PieceName, args.Points);
        }
    }
}
