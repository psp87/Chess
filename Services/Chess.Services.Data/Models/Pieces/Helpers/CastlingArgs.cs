namespace Chess.Services.Data.Models.Pieces.Helpers
{
    public class CastlingArgs
    {
        public bool IsCastlingMove { get; set; }

        public string RookSource { get; set; }

        public string RookTarget { get; set; }
    }
}
