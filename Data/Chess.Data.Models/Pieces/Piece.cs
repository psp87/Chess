namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces.Contracts;

    public abstract class Piece : IPiece, ICloneable
    {
        public string Id { get; set; }

        public string Name { get => this.GetType().Name.ToString(); }

        public Color Color { get; set; }

        public char Abbreviation { get; set; }

        public bool IsFirstMove { get; set; }

        public bool IsLastMove { get; set; }

        public abstract bool IsMoveAvailable();

        public abstract void Attacking();

        public abstract bool Move();

        public abstract bool Take();

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
