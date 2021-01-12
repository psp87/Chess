namespace Chess.Services.Data
{
    using System;
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
            return this.statsRepository.All().Where(x => x.OwnerId == userId).To<T>().FirstOrDefault();
        }

        public int GetUserRating(string userId)
        {
            return this.statsRepository.All().Where(x => x.OwnerId == userId).Select(x => x.Rating).FirstOrDefault();
        }

        public int GetTotalGames()
        {
            return this.statsRepository.All().Select(x => x.Games).Sum() / 2;
        }

        public string GetMostGamesUser()
        {
            int maxGames = this.statsRepository.All().Max(x => x.Games);

            return this.statsRepository.All().Where(x => x.Games == maxGames).Select(x => x.Owner.UserName).FirstOrDefault();
        }

        public string GetMostWinsUser()
        {
            int maxWins = this.statsRepository.All().Max(x => x.Wins);

            return this.statsRepository.All().Where(x => x.Wins == maxWins).Select(x => x.Owner.UserName).FirstOrDefault();
        }

        public void InitiateStats(string id)
        {
            var stats = new Stats
            {
                Games = 0,
                Wins = 0,
                Draws = 0,
                Losses = 0,
                OwnerId = id,
                Rating = 1200,
            };

            this.statsRepository.AddAsync(stats);
            this.statsRepository.SaveChangesAsync();
        }
    }
}
