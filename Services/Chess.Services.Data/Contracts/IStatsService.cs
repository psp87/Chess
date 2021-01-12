namespace Chess.Services.Data.Contracts
{
    public interface IStatsService
    {
        T GetUserStats<T>(string userId);

        int GetUserRating(string userId);

        int GetTotalUsers();

        int GetLastThirtyDaysRegisteredUsers();

        int GetTotalGames();

        string GetMostGamesUser();

        string GetMostWinsUser();

        void InitiateStats(string userId);
    }
}
