namespace Chess.Services.Data
{
    using System.Linq;

    using Chess.Data.Common.Repositories;
    using Chess.Data.Models;
    using Chess.Services.Data.Contracts;
    using Chess.Services.Mapping;

    public class StatsService : IStatsService
    {
        private readonly IRepository<Stats> statsRepository;

        public StatsService(IRepository<Stats> statsRepository)
        {
            this.statsRepository = statsRepository;
        }

        public T GetUserStats<T>(string userId)
        {
            return this.statsRepository.All().Where(x => x.ApplicationUserId == userId).To<T>().FirstOrDefault();
        }

        public int GetTotalUsers()
        {
            return this.statsRepository.All().Select(x => x.ApplicationUserId).Count();
        }

        public int GetTotalMatches()
        {
            return this.statsRepository.All().Select(x => x.Matches).Sum() / 2;
        }

        public string GetMostMatchesUser()
        {
            int maxGames = this.statsRepository.All().Max(x => x.Matches);
            return this.statsRepository.All().Where(x => x.Matches == maxGames).Select(x => x.ApplicationUser.UserName).FirstOrDefault();
        }

        public string GetMostWonsUser()
        {
            int maxWons = this.statsRepository.All().Max(x => x.Wons);
            return this.statsRepository.All().Where(x => x.Wons == maxWons).Select(x => x.ApplicationUser.UserName).FirstOrDefault();
        }

        public Stats GetUserStats(string id)
        {
            return this.statsRepository.All().Where(x => x.ApplicationUser.Id == id).FirstOrDefault();
        }

        public void InitiateStats(string id)
        {
            var stats = new Stats
            {
                Matches = 0,
                Wons = 0,
                Draws = 0,
                Losses = 0,
                ApplicationUserId = id,
            };
            this.statsRepository.AddAsync(stats);
            this.statsRepository.SaveChangesAsync();
        }
    }
}
