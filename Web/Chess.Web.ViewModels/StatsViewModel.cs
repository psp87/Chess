namespace Chess.Web.ViewModels
{
    public class StatsViewModel
    {
        public string UserName { get; set; }

        public UserStatsViewModel UserStats { get; set; }

        public int TotalUsers { get; set; }

        public int LastThirtyDaysRegisteredUsers { get; set; }

        public int TotalGames { get; set; }

        public string MostGamesUser { get; set; }

        public string MostWinsUser { get; set; }
    }
}
