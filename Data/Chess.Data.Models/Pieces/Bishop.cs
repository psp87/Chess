namespace Chess.Data.Models.Pieces
{
    using Chess.Common.Enums;
    using Chess.Data.Models.Pieces.Helpers;

    public class Bishop : Piece
    {
        private BishopBahaviour bishop;

        public Bishop(Color color)
            : base(color)
        {
            this.bishop = Factory.GetBishopBehaviour();
        }

        public override char Symbol => 'B';

        public override int Points => 3;

        public override void IsMoveAvailable(Square[][] matrix)
        {
            this.IsMovable = this.bishop.IsMoveAvailable(this, matrix) ? true : false;
        }

        public override void Attacking(Square[][] matrix)
        {
            this.bishop.Attacking(this, matrix);
        }

        public override bool Move(Position to, Square[][] matrix)
        {
            return this.bishop.Move(this, to, matrix);
        }

        public override bool Take(Position to, Square[][] matrix)
        {
            return this.Move(to, matrix);
        }

        public override object Clone()
        {
            return new Bishop(this.Color)
            {
                Position = this.Position.Clone() as Position,
                IsMovable = this.IsMovable,
            };
        }
    }
}
