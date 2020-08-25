namespace Chess.Data.Models.Pieces
{
    using Chess.Common.Enums;
    using Chess.Data.Models.Pieces.Helpers;

    public class Rook : Piece
    {
        private RookBehaviour rook;

        public Rook(Color color)
            : base(color)
        {
            this.rook = Factory.GetRookBehaviour();
        }

        public override char Symbol => 'R';

        public override int Points => 5;

        public override void IsMoveAvailable(Square[][] matrix)
        {
            this.IsMovable = this.rook.IsMoveAvailable(this, matrix) ? true : false;
        }

        public override void Attacking(Square[][] matrix)
        {
            this.rook.Attacking(this, matrix);
        }

        public override bool Move(Position to, Square[][] matrix)
        {
            return this.rook.Move(this, to, matrix);
        }

        public override bool Take(Position to, Square[][] matrix)
        {
            return this.Move(to, matrix);
        }

        public override object Clone()
        {
            return new Rook(this.Color)
            {
                Position = this.Position.Clone() as Position,
            };
        }
    }
}
