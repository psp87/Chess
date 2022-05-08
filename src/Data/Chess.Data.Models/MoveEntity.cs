namespace Chess.Data.Models
{
    using Chess.Data.Common.Models;

    public class MoveEntity : BaseModel<int>, IHaveOwner
    {
        public string Notation { get; set; }

        public string UserId { get; set; }

        public virtual UserEntity User { get; set; }

        public string GameId { get; set; }

        public virtual GameEntity Game { get; set; }
    }
}
