namespace Chess.Web.ViewModels
{
    using AutoMapper;
    using Chess.Data.Models;
    using Chess.Services.Mapping;

    public class UserStatsViewModel : IMapFrom<Stats>, IHaveCustomMappings
    {
        public int Games { get; set; }

        public int Wons { get; set; }

        public int Draws { get; set; }

        public int? Losses { get; set; }

        public double WonRatio { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Stats, UserStatsViewModel>().ForMember(
                m => m.WonRatio,
                opt => opt.MapFrom(u => (double)u.Wons / (u.Matches != 0 ? u.Matches : 1) * 100));
        }
    }
}
