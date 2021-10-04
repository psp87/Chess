namespace Chess.Services.Data.Contracts
{
    using System;

    using Chess.Services.Data.Models;

    public interface INotificationService
    {
        event EventHandler OnMove;

        event EventHandler OnUpdateHistory;

        void ClearCheck(Player movingPlayer, Player opponent);

        void Invalid(bool oldIsCheck, Player movingPlayer);

        void SendCheck(Player movingPlayer);

        void UpdateHistory(Player movingPlayer, string notation);
    }
}
