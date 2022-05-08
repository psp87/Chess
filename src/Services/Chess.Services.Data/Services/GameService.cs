namespace Chess.Services.Data.Services
{
    using System.Linq;

    using Chess.Data.Common.Repositories;
    using Chess.Data.Models;
    using Chess.Services.Data.Services.Contracts;

    public class GameService : IGameService
    {
        private readonly IDeletableEntityRepository<UserEntity> usersRepository;

        public GameService(IDeletableEntityRepository<UserEntity> usersRepository)
        {
            this.usersRepository = usersRepository;
        }

        public int GetCount()
        {
            return this.usersRepository.All().Count();
        }
    }
}
