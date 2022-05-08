namespace Chess.Services.Data.Models.Pieces
{
    using System;
    using System.Linq;

    using Chess.Common.Enums;
    using Chess.Services.Data.Models.Pieces.Contracts;

    public abstract class Piece : IPiece, ICloneable
    {
        public Piece(Color color)
        {
            this.Position = Factory.GetPosition();
            this.Color = color;
            this.IsFirstMove = true;
        }

        public Piece()
        {
            this.Position = Factory.GetPosition();
        }

        public string Name => this.GetType().Name.ToString();

        public Color Color { get; set; }

        public abstract char Symbol { get; }

        public abstract int Points { get; }

        public Position Position { get; set; }

        public bool IsFirstMove { get; set; }

        public bool IsLastMove { get; set; }

        public bool IsMovable { get; set; }

        public abstract void IsMoveAvailable(Square[][] boardMatrix);

        public abstract void Attacking(Square[][] boardMatrix);

        public abstract bool Move(Position toPosition, Square[][] boardMatrix, int turn, Move move);

        public abstract bool Take(Position toPosition, Square[][] boardMatrix, int turn, Move move);

        public bool IsType(params char[] pieceSymbols)
        {
            return pieceSymbols.Contains(this.Symbol);
        }

        public override string ToString()
        {
            return this.Position.ToString();
        }

        public abstract object Clone();
    }
}
