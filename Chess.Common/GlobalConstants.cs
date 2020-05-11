namespace Chess.Common
{
    public static class GlobalConstants
    {
        public const string SystemName = "Chess";

        public const string AdministratorRoleName = "Administrator";

        public const int BoardRows = 8;

        public const int BoardCols = 8;

        public static int TurnCounter = 0;

        public static GameOver GameOver { get; set; }
    }
}
