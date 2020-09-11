namespace Chess.Data.Models.Pieces.Helpers
{
    public static class EnPassant
    {
        public static int Turn { get; set; }

        public static Position Position { get; set; }

        public static string FenString { get; set; }
    }
}
