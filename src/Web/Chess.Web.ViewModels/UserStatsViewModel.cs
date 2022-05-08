namespace Chess.Web.ViewModels
{
    using Chess.Data.Models;
    using Chess.Services.Mapping;

    public class UserStatsViewModel : IMapFrom<StatisticEntity>
    {
        public int Games { get; set; }

        public int Wins { get; set; }

        public int Draws { get; set; }

        public int Losses { get; set; }

        public int Rating { get; set; }
    }
}
