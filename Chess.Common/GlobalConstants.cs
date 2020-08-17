namespace Chess.Common
{
    using Chess.Common.Enums;

    public static class GlobalConstants
    {
        public const string SystemName = "Chess";

        public const string AdministratorRoleName = "Administrator";

        public const int BoardRows = 8;

        public const int BoardCols = 8;

        public static int TurnCounter { get; set; }

        public static GameOver GameOver { get; set; }

        public static string EnPassantTake { get; set; }

        public static bool IsThreefoldDraw { get; set; }
    }
}
