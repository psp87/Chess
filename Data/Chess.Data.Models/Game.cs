namespace Chess.Data.Models
{
    using System.Collections.Generic;

    using Chess.Data.Common.Models;

    public class Game : BaseModel<string>
    {
        public string Player1 { get; set; }

        public string Player2 { get; set; }

        public virtual List<Move> Moves { get; set; } = new List<Move>();
    }
}
