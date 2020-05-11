namespace Chess.Models
{
    using System;
    using System.Collections.Generic;

    using Enums;
    using Pieces;
    using Pieces.Contracts;

    public class Square : ICloneable
    {
        private IPiece piece;

        public Square()
        {
            this.IsAttacked = new List<IPiece>();
            this.Position = Factory.GetPosition();
        }

        public Square(int posY, int posX) : this()
        {
            this.Position = Factory.GetPosition(posX, posY);
        }

        public IPiece Piece
        {
            get { return this.piece; }

            set
            {
                this.piece = value;
                if (this.piece != null)
                {
                    this.piece.Position.X = this.Position.X;
                    this.piece.Position.Y = this.Position.Y;

                    this.IsOccupied = this.piece is Empty ? false : true;
                }
            }
        }

        public Position Position { get; set; }

        public string Name { get; set; }

        public Color Color { get; set; }

        public bool IsOccupied { get; set; }

        public List<IPiece> IsAttacked { get; set; }

        public override string ToString()
        {
            return this.Piece?.Symbol + this.Name;
        }

        public object Clone()
        {
            return new Square()
            {
                Color = this.Color,
                Name = this.Name,
                Position = this.Position.Clone() as Position,
                Piece = this.Piece.Clone() as Piece,
            };
        }
    }
}
