namespace Chess.Data.Models.Pieces.Contracts
{
    using System;

    using Chess.Data.Models.Enums;

    public interface IPiece : ICloneable
    {
        string Id { get; set; }

        string Name { get; }

        Color Color { get; }

        char Abbreviation { get; }

        Position Position { get; set; }

        bool IsFirstMove { get; set; }

        bool IsLastMove { get; set; }

        bool IsMoveAvailable(Square[][] boardMatrix);

        void Attacking(Square[][] boardMatrix);

        bool Move(Position toPosition);

        bool Take(Position toPosition);

        string ToString();
    }
}
