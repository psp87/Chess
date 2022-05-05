namespace Chess.Data.Repositories
{
    using Chess.Data.Models;

    public class ErrorLogRepository : EfRepository<ErrorLogEntity>
    {
        public ErrorLogRepository(ChessDbContext context)
            : base(context)
        {
        }
    }
}
