namespace Chess.Data.Models
{
    public class Move
    {
        public int Id { get; set; }

        public string Notation { get; set; }

        public string GameId { get; set; }

        public virtual Game Game { get; set; }
    }
}
