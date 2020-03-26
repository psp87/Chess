namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;

    public class Knight : Piece, ICloneable
    {
        public Knight(Color color)
            : base(color)
        {
        }

        public override char Abbreviation => 'N';

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
