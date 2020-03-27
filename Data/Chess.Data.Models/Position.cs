namespace Chess.Data.Models
{
    using System;

    using Chess.Data.Models.Enums;

    public class Position : ICloneable
    {
        public Position(X posX, Y posY)
        {
            this.X = posX;
            this.Y = posY;
        }

        public X X { get; set; }

        public Y Y { get; set; }

        public bool IsInBoard()
        {
            return this.X >= X.A && this.X <= X.H && this.Y >= Y.One && this.Y <= Y.Eight;
        }

        public override string ToString()
        {
            return "[" + this.X + ", " + this.Y + "]";
        }

        public object Clone()
        {
            return new Position(this.X, this.Y);
        }
    }
}
