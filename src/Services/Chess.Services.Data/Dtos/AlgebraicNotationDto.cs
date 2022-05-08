namespace Chess.Services.Data.Dtos
{
    using Chess.Services.Data.Models;

    public class AlgebraicNotationDto
    {
        public Square OldSource { get; set; }

        public Square OldTarget { get; set; }

        public Board OldBoard { get; set; }

        public Player Opponent { get; set; }

        public int Turn { get; set; }

        public Move Move { get; set; }
    }
}
