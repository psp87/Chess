namespace Chess.Services.Data.Services.Contracts
{
    public interface IStatsService
    {
        T GetUserStats<T>(string userId);

        bool IsStatsInitiated(string userId);

        int GetUserRating(string userId);

        int GetTotalGames();

        string GetMostGamesUser();

        string GetMostWinsUser();

        void InitiateStats(string userId);
    }
}
