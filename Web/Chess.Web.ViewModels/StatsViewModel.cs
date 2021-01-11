namespace Chess.Web.ViewModels
{
    public class StatsViewModel
    {
        public string Name { get; set; }

        public UserStatsViewModel UserStats { get; set; }

        public int TotalUsers { get; set; }

        public int TotalMatches { get; set; }

        public string MostMatchesUser { get; set; }

        public string MostWonsUser { get; set; }
    }
}
