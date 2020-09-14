namespace Chess.Data.Models
{
    using System;
    using System.Collections.Generic;

    using Chess.Common.Enums;
    using Chess.Data.Models.Pieces;
    using Chess.Data.Models.Pieces.Contracts;

    public class Square : ICloneable
    {
        private IPiece piece;

        public Square()
        {
            this.IsAttacked = new List<IPiece>();
            this.Position = Factory.GetPosition();
        }

        public Square(int posY, int posX)
            : this()
        {
            this.Position = Factory.GetPosition(posY, posX);
        }

        public IPiece Piece
        {
            get => this.piece;

            set
            {
                this.piece = value;
                if (this.piece != null)
                {
                    this.piece.Position.X = this.Position.X;
                    this.piece.Position.Y = this.Position.Y;
                }
            }
        }

        public Position Position { get; set; }

        public string Name { get; set; }

        public Color Color { get; set; }

        public List<IPiece> IsAttacked { get; set; }

        public override string ToString()
        {
            return this.Name.ToLower();
        }

        public override bool Equals(object obj)
        {
            Square other = (Square)obj;
            return this.Position.X == other.Position.X && this.Position.Y == other.Position.Y;
        }

        public object Clone()
        {
            return new Square()
            {
                Color = this.Color,
                Name = this.Name,
                Position = this.Position.Clone() as Position,
                Piece = this.Piece?.Clone() as Piece,
                IsAttacked = this.IsAttacked,
            };
        }
    }
}
