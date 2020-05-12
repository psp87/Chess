namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Common.Enums;

    public class Empty : Piece
    {
        public Empty()
            : base()
        {
            this.Color = Color.Empty;
        }

        public override char Symbol => '-';

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
