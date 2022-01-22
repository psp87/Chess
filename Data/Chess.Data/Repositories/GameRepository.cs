namespace Chess.Data.Repositories
{
    using Chess.Data.Models;

    public class GameRepository : EfRepository<Game>
    {
        public GameRepository(ChessDbContext context)
            : base(context)
        {
        }
    }
}
