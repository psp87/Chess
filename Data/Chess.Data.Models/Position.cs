namespace Chess.Models
{
    using System;

    public class Position : ICloneable
    {
        public Position() : this(-1, -1)
        {
        }

        public Position(int posY, int posX)
        {
            this.Y = posY;
            this.X = posX;
        }

        public int X { get; set; }

        public int Y { get; set; }

        public static bool IsInBoard(int x, int y)
        {
            return x >= 0 && x <= 7 && y >= 0 && y <= 7;
        }

        public override string ToString()
        {
            return "[" + this.Y + ", " + this.X + "]";
        }

        public object Clone()
        {
            return Factory.GetPosition(this.Y, this.X);
        }
    }
}
