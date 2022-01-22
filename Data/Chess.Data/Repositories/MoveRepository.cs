namespace Chess.Data.Repositories
{
    using Chess.Data.Models;

    public class MoveRepository : EfRepository<Move>
    {
        public MoveRepository(ChessDbContext context)
            : base(context)
        {
        }
    }
}
