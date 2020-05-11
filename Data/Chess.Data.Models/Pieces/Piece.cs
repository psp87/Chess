namespace Chess.Models.Pieces
{
    using System;

    using Contracts;
    using Enums;

    public abstract class Piece : IPiece, ICloneable
    {
        public Piece(Color color)
        {
            Position = Factory.GetPosition();
            this.Color = color;
            this.IsFirstMove = true;
        }

        public Piece()
        {
            Position = Factory.GetPosition();
        }

        public string Name => this.GetType().Name.ToString();

        public Color Color { get; set; }

        public abstract char Symbol { get; }

        public abstract bool[,] FigureMatrix { get; }

        public Position Position { get; set; }

        public bool IsFirstMove { get; set; }

        public bool IsLastMove { get; set; }

        public bool IsMovable { get; set; }

        public abstract void IsMoveAvailable(Square[][] boardMatrix);

        public abstract void Attacking(Square[][] boardMatrix);

        public abstract bool Move(Position toPosition, Square[][] boardMatrix);

        public abstract bool Take(Position toPosition, Square[][] boardMatrix);

        public override string ToString()
        {
            return Position.ToString();
        }

        public virtual object Clone()
        {
            return this.Clone();
        }
    }
}
