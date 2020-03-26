namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;

    public class Bishop : Piece, ICloneable
    {
        public Bishop(Color color)
            : base(color)
        {
        }

        public override char Abbreviation => 'B';

        public override bool IsMoveAvailable()
        {
            throw new NotImplementedException();
        }

        public override void Attacking()
        {
            throw new NotImplementedException();
        }

        public override bool Move()
        {
            throw new NotImplementedException();
        }

        public override bool Take()
        {
            throw new NotImplementedException();
        }
    }
}
