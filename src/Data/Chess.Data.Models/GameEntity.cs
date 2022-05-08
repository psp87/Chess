namespace Chess.Data.Models
{
    using System.Collections.Generic;

    using Chess.Data.Common.Models;

    public class GameEntity : BaseModel<string>
    {
        public string PlayerOneName { get; set; }

        public string PlayerOneUserId { get; set; }

        public string PlayerTwoName { get; set; }

        public string PlayerTwoUserId { get; set; }

        public virtual List<MoveEntity> Moves { get; set; } = new List<MoveEntity>();
    }
}
