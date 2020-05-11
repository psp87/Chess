namespace Chess.Models
{
    public class Move
    {
        public Move()
        {
            this.Start = Factory.GetSquare();
            this.End = Factory.GetSquare();
        }

        public Move(Move move)
        {
            this.Start = Factory.GetSquare(move.Start.Position.X, move.Start.Position.Y);
            this.End = Factory.GetSquare(move.End.Position.X, move.End.Position.Y);
        }

        public char Symbol { get; set; }

        public Square Start { get; set; }

        public Square End { get; set; }

        public override string ToString()
        {
            return this.Start.ToString() + this.End.ToString();
        }
    }
}
