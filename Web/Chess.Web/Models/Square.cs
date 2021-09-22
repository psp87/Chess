namespace Chess.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Chess.Common.Enums;
    using Chess.Web.Models.Pieces;
    using Chess.Web.Models.Pieces.Contracts;

    public class Square : ICloneable
    {
        private IPiece piece;

        public Square()
        {
            this.IsAttacked = new List<IPiece>();
            this.Position = Factory.GetPosition();
        }

        public Square(int rank, int file)
            : this()
        {
            this.Position = Factory.GetPosition(rank, file);
        }

        public IPiece Piece
        {
            get => this.piece;

            set
            {
                this.piece = value;
                if (this.piece != null)
                {
                    this.piece.Position.File = this.Position.File;
                    this.piece.Position.Rank = this.Position.Rank;
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
            return this.Position.File == other.Position.File && this.Position.Rank == other.Position.Rank;
        }

        public override int GetHashCode()
        {
            return this.Position.File.GetHashCode() + this.Position.Rank.GetHashCode();
        }

        public object Clone()
        {
            return new Square()
            {
                Color = this.Color,
                Name = this.Name,
                Position = this.Position.Clone() as Position,
                Piece = this.Piece?.Clone() as Piece,
                IsAttacked = this.IsAttacked.ToList(),
            };
        }
    }
}
