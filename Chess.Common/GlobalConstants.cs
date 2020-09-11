namespace Chess.Common
{
    public static class GlobalConstants
    {
        public const string SystemName = "Chess";

        public const string AdministratorRoleName = "Administrator";

        public const int BoardRows = 8;

        public const int BoardCols = 8;

        public static string EnPassantTake { get; set; }

        public static bool CastlingMove { get; set; }

        public static string PawnPromotionFen { get; set; }
    }
}
