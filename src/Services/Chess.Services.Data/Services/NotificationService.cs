namespace Chess.Services.Data.Services
{
    using System;

    using Chess.Common.Enums;
    using Chess.Common.EventArgs;
    using Chess.Services.Data.Models;
    using Chess.Services.Data.Services.Contracts;

    public class NotificationService : INotificationService
    {
        public event EventHandler OnGameOver;

        public event EventHandler OnTakePiece;

        public event EventHandler OnAvailableThreefoldDraw;

        public event EventHandler OnMoveEvent;

        public event EventHandler OnCompleteMove;

        public void InvalidMove(bool oldCheck, Player movingPlayer)
        {
            if (movingPlayer.IsCheck && oldCheck)
            {
                this.OnMoveEvent?.Invoke(
                    movingPlayer,
                    new MoveArgs(Message.CheckSelf));
                return;
            }

            if (movingPlayer.IsCheck && !oldCheck)
            {
                this.OnMoveEvent?.Invoke(
                    movingPlayer,
                    new MoveArgs(Message.CheckOpen));

                movingPlayer.IsCheck = false;
                return;
            }

            this.OnMoveEvent?.Invoke(
                movingPlayer,
                new MoveArgs(Message.InvalidMove));
        }

        public void SendCheck(Player movingPlayer)
        {
            this.OnMoveEvent?.Invoke(
                movingPlayer,
                new MoveArgs(Message.CheckOpponent));
        }

        public void ClearCheck(Player movingPlayer, Player opponent)
        {
            if (!movingPlayer.IsCheck && !opponent.IsCheck)
            {
                this.OnMoveEvent?.Invoke(
                    movingPlayer,
                    new MoveArgs(Message.CheckClear));
            }
        }

        public void UpdateMoveHistory(Player movingPlayer, string notation)
        {
            this.OnCompleteMove?.Invoke(
                movingPlayer,
                new HistoryUpdateArgs(notation));
        }

        public void UpdateTakenPiecesHistory(Player movingPlayer, string pieceName)
        {
            this.OnTakePiece?.Invoke(movingPlayer, new TakePieceEventArgs(pieceName, movingPlayer.Points));
        }

        public void SendThreefoldDrawAvailability(Player movingPlayer, bool available)
        {
            this.OnAvailableThreefoldDraw?.Invoke(movingPlayer, new ThreefoldDrawEventArgs(available));
        }

        public void SendGameOver(Player movingPlayer, GameOver gameOver)
        {
            this.OnGameOver?.Invoke(movingPlayer, new GameOverEventArgs(gameOver));
        }
    }
}
