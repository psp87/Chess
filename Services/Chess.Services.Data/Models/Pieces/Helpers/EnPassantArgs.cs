namespace Chess.Services.Data.Models.Pieces.Helpers
{
    public class EnPassantArgs
    {
        public int Turn { get; set; }

        public Square SquareAvailable { get; set; }

        public Square SquareTakenPiece { get; set; }
    }
}
