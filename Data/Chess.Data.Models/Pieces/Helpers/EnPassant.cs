namespace Chess.Data.Models.Pieces.Helpers
{
    public class EnPassant
    {
        public int Turn { get; set; }

        public Position Position { get; set; }

        public string FenString { get; set; }
    }
}
