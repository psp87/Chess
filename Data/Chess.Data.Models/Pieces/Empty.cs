namespace Chess.Models.Pieces
{
    using System;

    using Enums;

    public class Empty : Piece
    {
        public Empty()
            : base()
        {
            this.Color = Color.Empty;
        }

        public override char Symbol => '-';

        public override bool[,] FigureMatrix 
        { 
            get => new bool[Globals.CellRows, Globals.CellCols]
            {
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false }
            };
        }

        public override void IsMoveAvailable(Square[][] matrix)
        {
            throw new NotImplementedException();
        }

        public override void Attacking(Square[][] matrix)
        {
            throw new NotImplementedException();
        }

        public override bool Move(Position toPos, Square[][] matrix)
        {
            throw new NotImplementedException();
        }

        public override bool Take(Position toPos, Square[][] matrix)
        {
            throw new NotImplementedException();
        }
    }
}
