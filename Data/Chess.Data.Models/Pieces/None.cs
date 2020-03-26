namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;

    public class None : Piece, ICloneable
    {
        public None()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Color = Color.None;
            this.Abbreviation = 'E';
            this.IsFirstMove = false;
            this.IsLastMove = false;
        }

        public override void Attacking()
        {
            throw new NotImplementedException();
        }

        public override bool IsMoveAvailable()
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
