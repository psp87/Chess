namespace Chess.Services.Data.Models.Pieces.Helpers
{
    using Chess.Web.Models;

    public class EnPassant
    {
        public int Turn { get; set; }

        public Square SquareAvailable { get; set; }

        public Square SquareTakenPiece { get; set; }
    }
}
