namespace Chess.Services.Data.Contracts
{
    using System;

    using Chess.Services.Data.Models;

    public interface INotificationService
    {
        event EventHandler OnMove;

        event EventHandler OnHistoryUpdate;

        void ClearCheck(Player movingPlayer, Player opponent);

        void Invalid(bool oldIsCheck, Player movingPlayer);

        void SendCheck(Player movingPlayer);

        void UpdateMoveHistory(Square source, Square target, Board board, Player movingPlayer, Player opponent, int turn, Move move);
    }
}
