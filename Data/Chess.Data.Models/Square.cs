namespace Chess.Data.Models
{
    using System;
    using System.Collections.Generic;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces;
    using Chess.Data.Models.Pieces.Contracts;

    public class Square : ICloneable
    {
        private IPiece piece;

        public Square()
        {
            this.IsAttacked = new List<IPiece>();
        }

        public IPiece Piece
        {
            get { return this.piece; }

            set
            {
                this.piece = value;
                if (this.piece != null)
                {
                    this.piece.Position.PosX = this.Position.PosX;
                    this.piece.Position.PosY = this.Position.PosY;
                }
            }
        }

        public Position Position { get; set; }

        public string Name { get; set; }

        public Color Color { get; set; }

        public List<IPiece> IsAttacked { get; set; }

        public override string ToString()
        {
            return this.Piece?.Abbreviation + this.Name;
        }

        public object Clone()
        {
            return new Square()
            {
                Color = this.Color,
                Name = this.Name,
                Position = this.Position.Clone() as Position,
                Piece = this.Piece?.Clone() as Piece,
            };
        }
    }
}
