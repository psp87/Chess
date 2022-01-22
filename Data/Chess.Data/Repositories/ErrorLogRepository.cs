namespace Chess.Data.Repositories
{
    using Chess.Data.Models;

    public class ErrorLogRepository : EfRepository<ErrorLog>
    {
        public ErrorLogRepository(ChessDbContext context)
            : base(context)
        {
        }
    }
}
