namespace Chess.Data.Models.Pieces.Helpers
{
    public class Castling
    {
        public bool IsCastlingMove { get; set; }

        public string RookSource { get; set; }

        public string RookTarget { get; set; }
    }
}
