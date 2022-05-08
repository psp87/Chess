namespace Chess.Data.Repositories
{
    using Chess.Data.Models;

    public class StatisticRepository : EfRepository<StatisticEntity>
    {
        public StatisticRepository(ChessDbContext context)
            : base(context)
        {
        }
    }
}
