namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces.Contracts;

    public abstract class Piece : IPiece, ICloneable
    {
        public Piece(Color color)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Color = color;
            this.IsFirstMove = true;
            this.IsLastMove = false;
        }

        public string Id { get; set; }

        public string Name => this.GetType().Name.ToString();

        public Color Color { get; set; }

        public abstract char Abbreviation { get; }

        public Position Position { get; set; }

        public bool IsFirstMove { get; set; }

        public bool IsLastMove { get; set; }

        public abstract bool IsMoveAvailable();

        public abstract void Attacking();

        public abstract bool Move();

        public abstract bool Take();

        public override string ToString()
        {
            return this.Position.ToString();
        }

        public virtual object Clone()
        {
            return this.Clone();
        }
    }
}
