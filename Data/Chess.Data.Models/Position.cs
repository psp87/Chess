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

        public static bool IsInBoard(X x, Y y)
        {
            return x >= X.A && x <= X.H && y >= Y.One && y <= Y.Eight;
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
