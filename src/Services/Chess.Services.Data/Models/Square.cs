namespace Chess.Services.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Chess.Common.Enums;
    using Chess.Services.Data.Models.Pieces;
    using Chess.Services.Data.Models.Pieces.Contracts;

    public class Square : ICloneable
    {
        private IPiece piece;

        public Square()
        {
        }

        public Square(int rank, int file)
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

        public Position Position { get; set; } = Factory.GetPosition();

        public string Name { get; set; }

        public Color Color { get; set; }

        public List<IPiece> IsAttacked { get; set; } = new List<IPiece>();

        public bool SameAllyPieceAttack(IPiece piece)
            => this.IsAttacked
                .Count(x =>
                    x.Color == piece.Color &&
                    x.Name.Equals(piece.Name))
                > 1;

        public bool IsAttackedByColor(Color color)
            => this.IsAttacked
                .Where(x => x.Color == color)
                .Any();

        public bool IsAttackedByPiece(Color color, params char[] pieceSymbols)
            => this.IsAttacked
                .Where(x => x.Color == color && x.IsType(pieceSymbols))
                .Any();

        public override string ToString()
        {
            return this.Name.ToLower();
        }

        public override bool Equals(object obj)
        {
            var other = (Square)obj;
            return this.Position.File == other.Position.File && this.Position.Rank == other.Position.Rank;
        }

        public override int GetHashCode()
        {
            return this.Position.File.GetHashCode() + this.Position.Rank.GetHashCode();
        }

        public object Clone()
            => new Square()
            {
                Color = this.Color,
                Name = this.Name,
                Position = this.Position.Clone() as Position,
                Piece = this.Piece?.Clone() as Piece,
                IsAttacked = this.IsAttacked.ToList(),
            };
    }
}
