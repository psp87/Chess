namespace Chess.Data.Repositories
{
    using Chess.Data.Models;

    public class MoveRepository : EfRepository<MoveEntity>
    {
        public MoveRepository(ChessDbContext context)
            : base(context)
        {
        }
    }
}
