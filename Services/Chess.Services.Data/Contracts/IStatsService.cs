namespace Chess.Services.Data.Contracts
{
    using Chess.Data.Models;

    public interface IStatsService
    {
        T GetUserStats<T>(string userId);

        int GetTotalUsers();

        int GetTotalMatches();

        string GetMostMatchesUser();

        string GetMostWonsUser();

        Stats GetUserStats(string id);

        void InitiateStats(string id);
    }
}
