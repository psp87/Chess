namespace Chess.Data.Repositories
{
    using Chess.Data.Models;

    public class GameRepository : EfRepository<GameEntity>
    {
        public GameRepository(ChessDbContext context)
            : base(context)
        {
        }
    }
}
