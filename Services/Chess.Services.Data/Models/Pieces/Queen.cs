namespace Chess.Services.Data.Models.Pieces
{
    using Chess.Common.Constants;
    using Chess.Common.Enums;
    using Chess.Services.Data.Models.Pieces.Helpers;

    public class Queen : Piece
    {
        private readonly RookBehaviour rook;
        private readonly BishopBahaviour bishop;

        public Queen(Color color)
            : base(color)
        {
            this.rook = Factory.GetRookBehaviour();
            this.bishop = Factory.GetBishopBehaviour();
        }

        public override char Symbol => SymbolConstants.Queen;

        public override int Points => PointsConstants.Queen;

        public override void IsMoveAvailable(Square[][] matrix)
        {
            if (this.bishop.IsMoveAvailable(this, matrix) ||
                this.rook.IsMoveAvailable(this, matrix))
            {
                this.IsMovable = true;
            }
            else
            {
                this.IsMovable = false;
            }
        }

        public override void Attacking(Square[][] matrix)
        {
            this.rook.Attacking(this, matrix);
            this.bishop.Attacking(this, matrix);
        }

        public override bool Move(Position to, Square[][] matrix, int turn, Move move)
        {
            if (this.bishop.Move(this, to, matrix, move) || this.rook.Move(this, to, matrix, move))
            {
                return true;
            }

            return false;
        }

        public override bool Take(Position to, Square[][] matrix, int turn, Move move)
        {
            return this.Move(to, matrix, turn, move);
        }

        public override object Clone()
        {
            return new Queen(this.Color)
            {
                Position = this.Position.Clone() as Position,
                IsMovable = this.IsMovable,
            };
        }
    }
}
