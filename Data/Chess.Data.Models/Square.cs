namespace Chess.Data.Models
{
    using System;
    using System.Collections.Generic;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces.Contracts;

    public class Square : ICloneable
    {
        public Square(PositionX positionX, PositionY positionY)
        {
            this.PositionX = positionX;
            this.PositionY = positionY;
            this.Piece = Factory.GetNone();
            this.IsOccupied = false;
            this.IsAttacked = new List<Square>();
        }

        public PositionX PositionX { get; set; }

        public PositionY PositionY { get; set; }

        public Color Color { get; set; }

        public IPiece Piece { get; set; }

        public bool IsOccupied { get; set; }

        public List<Square> IsAttacked { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
