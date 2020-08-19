namespace Chess.Data.Models
{
    public class Move
    {
        public Move()
        {
            this.Start = Factory.GetSquare();
            this.End = Factory.GetSquare();
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
