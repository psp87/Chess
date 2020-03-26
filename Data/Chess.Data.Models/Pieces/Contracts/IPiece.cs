namespace Chess.Data.Models.Pieces.Contracts
{
    using Chess.Data.Models.Enums;

    public interface IPiece
    {
        string Id { get; set; }

        string Name { get; }

        Color Color { get; }

        char Abbreviation { get; set; }

        bool IsFirstMove { get; set; }

        bool IsLastMove { get; set; }

        bool IsMoveAvailable();

        void Attacking();

        bool Move();

        bool Take();
    }
}
