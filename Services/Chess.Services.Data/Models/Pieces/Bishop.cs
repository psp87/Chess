namespace Chess.Services.Data.Models.Pieces
{
    using Chess.Common.Constants;
    using Chess.Common.Enums;
    using Chess.Services.Data.Models.Pieces.Helpers;

    public class Bishop : Piece
    {
        private readonly BishopBahaviour bishop;

        public Bishop(Color color)
            : base(color)
        {
            this.bishop = Factory.GetBishopBehaviour();
        }

        public override char Symbol => SymbolConstants.Bishop;

        public override int Points => PointsConstants.Bishop;

        public override void IsMoveAvailable(Square[][] matrix)
        {
            this.IsMovable = this.bishop.IsMoveAvailable(this, matrix);
        }

        public override void Attacking(Square[][] matrix)
        {
            this.bishop.Attacking(this, matrix);
        }

        public override bool Move(Position to, Square[][] matrix, int turn, Move move)
        {
            return this.bishop.Move(this, to, matrix, move);
        }

        public override bool Take(Position to, Square[][] matrix, int turn, Move move)
        {
            return this.Move(to, matrix, turn, move);
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
