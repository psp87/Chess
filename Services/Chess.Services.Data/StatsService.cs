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
            return this.statsRepository.All().Where(x => x.UserId == userId).To<T>().FirstOrDefault();
        }

        public bool IsStatsInitiated(string id)
        {
            return this.statsRepository.All().Where(x => x.User.Id == id).Any();
        }

        public int GetUserRating(string userId)
        {
            return this.statsRepository.All().Where(x => x.UserId == userId).Select(x => x.EloRating).FirstOrDefault();
        }

        public int GetTotalGames()
        {
            return this.statsRepository.All().Select(x => x.Games).Sum() / 2;
        }

        public string GetMostGamesUser()
        {
            int maxGames = this.statsRepository.All().Max(x => x.Games);

            return this.statsRepository.All().Where(x => x.Games == maxGames).Select(x => x.User.UserName).FirstOrDefault();
        }

        public string GetMostWinsUser()
        {
            int maxWins = this.statsRepository.All().Max(x => x.Win);

            return this.statsRepository.All().Where(x => x.Win == maxWins).Select(x => x.User.UserName).FirstOrDefault();
        }

        public void InitiateStats(string id)
        {
            var stats = new Stats
            {
                Games = 0,
                Win = 0,
                Draw = 0,
                Loss = 0,
                UserId = id,
                EloRating = 1200,
            };

            this.statsRepository.AddAsync(stats);
            this.statsRepository.SaveChangesAsync();
        }
    }
}
