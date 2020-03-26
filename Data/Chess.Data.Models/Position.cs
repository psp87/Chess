namespace Chess.Data.Models
{
    using System;

    using Chess.Data.Models.Enums;

    public class Position : ICloneable
    {
        public Position(PosX posX, PosY posY)
        {
            this.PosX = posX;
            this.PosY = posY;
        }

        public PosX PosX { get; set; }

        public PosY PosY { get; set; }

        public bool IsInBoard()
        {
            return this.PosX >= PosX.A && this.PosX <= PosX.H && this.PosY >= PosY.One && this.PosY <= PosY.Eight;
        }

        public override string ToString()
        {
            return "[" + this.PosX + ", " + this.PosY + "]";
        }

        public object Clone()
        {
            return new Position(this.PosX, this.PosY);
        }
    }
}
