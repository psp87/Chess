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
        #region Private Variables
        private readonly ConcurrentDictionary<string, Player> players =
            new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentQueue<Player> waitingPlayers =
            new ConcurrentQueue<Player>();
        #endregion

        public Game Game { get; set; }

        public async Task FindGame(string username)
        {
            Player joiningPlayer = Factory.GetPlayer(username, this.Context.ConnectionId);
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
                joiningPlayer.Color = Color.Dark;
                opponent.Color = Color.Light;
                opponent.HasToMove = true;

                this.Game = Factory.GetGame(opponent, joiningPlayer);

                this.Game.OnGameOver += this.Game_OnGameOver;
                this.Game.OnNotification += this.Game_OnNotification;
                this.Game.ChessBoard.OnTakePiece += this.Board_OnTakePiece;

                await Task.WhenAll(
                    this.Groups.AddToGroupAsync(this.Game.Player1.Id, groupName: this.Game.Id),
                    this.Groups.AddToGroupAsync(this.Game.Player2.Id, groupName: this.Game.Id),
                    this.Clients.Group(this.Game.Id).SendAsync("Start", this.Game));

                await this.Clients.Caller.SendAsync("ChangeOrientation");
            }
        }

        public async Task MoveSelected(string source, string target, string sourceFen, string targetFen)
        {
            var player = this.players[this.Context.ConnectionId];

            if (!player.HasToMove ||
                !this.Game.MakeMove(source, target, targetFen))
            {
                await this.Clients.Caller.SendAsync("BoardSnapback", sourceFen);
                return;
            }

            await this.Clients.Others.SendAsync("BoardMove", source, target);

            if (GlobalConstants.GameOver.ToString() == GameOver.None.ToString())
            {
                await this.Clients.All.SendAsync("UpdateStatus", this.Game.MovingPlayer.Name);
            }

            if (GlobalConstants.EnPassantTake != null)
            {
                await this.Clients.All.SendAsync("EnPassantTake", GlobalConstants.EnPassantTake, target);
                GlobalConstants.EnPassantTake = null;
            }

            if (GlobalConstants.CastlingMove)
            {
                await this.Clients.All.SendAsync("BoardMove", Castling.RookSource, Castling.RookTarget);
                GlobalConstants.CastlingMove = false;
            }

            if (GlobalConstants.PawnPromotionFen != null)
            {
                await this.Clients.All.SendAsync("BoardSetPosition", GlobalConstants.PawnPromotionFen);
                GlobalConstants.PawnPromotionFen = null;
            }
        }

        public async Task IsThreefoldDraw()
        {
            var player = this.players[this.Context.ConnectionId];

            if (GlobalConstants.IsThreefoldDraw && player.HasToMove)
            {
                await this.Clients.All.SendAsync("GameOver", player, GameOver.ThreefoldDraw);
            }
        }

        public async Task Resign()
        {
            var player = this.players[this.Context.ConnectionId];

            await this.Clients.All.SendAsync("GameOver", player, GameOver.Resign);
        }

        public async Task OfferDrawRequest()
        {
            var player = this.players[this.Context.ConnectionId];

            await this.Clients.Others.SendAsync("DrawOffered", player);
        }

        public async Task OfferDrawAnswer(bool isAccepted)
        {
            var player = this.players[this.Context.ConnectionId];

            if (isAccepted)
            {
                await this.Clients.All.SendAsync("GameOver", null, GameOver.Draw);
            }
            else
            {
                await this.Clients.Others.SendAsync("DrawOfferRejected", player);
            }
        }

        private void Game_OnGameOver(object sender, EventArgs e)
        {
            var player = sender as Player;
            var gameOver = e as GameOverEventArgs;

            this.Clients.All.SendAsync("GameOver", player, gameOver.GameOver);
        }

        private void Game_OnNotification(object sender, EventArgs e)
        {
            var player = sender as Player;
            var notification = e as NotificationEventArgs;

            switch (notification.Type)
            {
                case Notification.InvalidMove:
                    this.Clients.Caller.SendAsync("InvalidMessage");
                    break;
                case Notification.CheckClear:
                    this.Clients.All.SendAsync("EmptyCheckStatus");
                    break;
                case Notification.CheckOpponent:
                    this.Clients.All.SendAsync("CheckOpponent");
                    break;
                case Notification.CheckSelf:
                    this.Clients.Caller.SendAsync("CheckSelf");
                    break;
            }
        }

        private void Board_OnTakePiece(object sender, EventArgs e)
        {
            var player = sender as Player;
            var args = e as TakePieceEventArgs;

            this.Clients.All.SendAsync("UpdateTakenFigures", player, args.PieceName, args.Points);
        }
    }
}
