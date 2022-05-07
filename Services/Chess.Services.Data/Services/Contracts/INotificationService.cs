namespace Chess.Services.Data.Services.Contracts
{
    using System;

    using Chess.Common.Enums;
    using Chess.Services.Data.Models;

    public interface INotificationService
    {
        event EventHandler OnGameOver;

        event EventHandler OnTakePiece;

        event EventHandler OnAvailableThreefoldDraw;

        event EventHandler OnMoveEvent;

        event EventHandler OnCompleteMove;

        void ClearCheck(Player movingPlayer, Player opponent);

        void InvalidMove(bool oldIsCheck, Player movingPlayer);

        void SendCheck(Player movingPlayer);

        void UpdateMoveHistory(Player movingPlayer, string notation);

        void UpdateTakenPiecesHistory(Player movingPlayer, string pieceName);

        void SendThreefoldDrawAvailability(Player movingPlayer, bool available);

        void SendGameOver(Player movingPlayer, GameOver gameOver);
    }
}
