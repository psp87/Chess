namespace Chess.Services.Data.Services.Contracts
{
    using Chess.Services.Data.Models;

    public interface IDrawService
    {
        bool IsStalemate(Board board, Player opponent);

        bool IsDraw(Board board);

        bool IsThreefoldRepetionDraw(string fen);

        bool IsFivefoldRepetitionDraw(string fen);

        bool IsFiftyMoveDraw(Move move);
    }
}
