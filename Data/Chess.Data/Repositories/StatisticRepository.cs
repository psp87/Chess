namespace Chess.Data.Repositories
{
    using Chess.Data.Models;

    public class StatisticRepository : EfRepository<Game>
    {
        public StatisticRepository(ChessDbContext context)
            : base(context)
        {
        }
    }
}
