namespace Chess.Services.Data
{
    using System;

    using Chess.Services.Data.Contracts;
    using Chess.Services.Data.Models;
    using Chess.Services.Data.Models.EventArgs;
    using Common.Enums;

    public class NotificationService : INotificationService
    {
        public event EventHandler OnMove;

        public event EventHandler OnUpdateHistory;

        public void Invalid(bool oldCheck, Player movingPlayer)
        {
            if (movingPlayer.IsCheck && oldCheck)
            {
                this.OnMove?.Invoke(
                    movingPlayer,
                    new MoveArgs(Message.CheckSelf));
                return;
            }

            if (movingPlayer.IsCheck && !oldCheck)
            {
                this.OnMove?.Invoke(
                    movingPlayer,
                    new MoveArgs(Message.CheckOpen));

                movingPlayer.IsCheck = false;
                return;
            }

            this.OnMove?.Invoke(
                movingPlayer,
                new MoveArgs(Message.InvalidMove));
        }

        public void SendCheck(Player movingPlayer)
        {
            this.OnMove?.Invoke(
                movingPlayer,
                new MoveArgs(Message.CheckOpponent));
        }

        public void ClearCheck(Player movingPlayer, Player opponent)
        {
            if (!movingPlayer.IsCheck && !opponent.IsCheck)
            {
                this.OnMove?.Invoke(
                    movingPlayer,
                    new MoveArgs(Message.CheckClear));
            }
        }

        public void UpdateHistory(Player movingPlayer, string notation)
        {
            this.OnUpdateHistory?.Invoke(
                movingPlayer,
                new HistoryUpdateArgs(notation));
        }
    }
}
