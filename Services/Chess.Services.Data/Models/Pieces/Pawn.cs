namespace Chess.Services.Data.Models.Pieces
{
    using Chess.Common.Constants;
    using Chess.Common.Enums;

    public class Pawn : Piece
    {
        public Pawn(Color color)
            : base(color)
        {
        }

        public override char Symbol => SymbolConstants.Pawn;

        public override int Points => PointsConstants.Pawn;

        public override void IsMoveAvailable(Square[][] matrix)
        {
            int offsetPlayer = this.Color == Color.White ? -1 : 1;

            if (Position.IsInBoard(this.Position.File, this.Position.Rank + (offsetPlayer * 1)))
            {
                var checkedSquare = matrix[this.Position.Rank + (offsetPlayer * 1)][this.Position.File];

                if (checkedSquare.Piece == null)
                {
                    this.IsMovable = true;
                    return;
                }
            }

            if (Position.IsInBoard(this.Position.File - 1, this.Position.Rank + (offsetPlayer * 1)))
            {
                var checkedSquare = matrix[this.Position.Rank + (offsetPlayer * 1)][this.Position.File - 1];

                if (checkedSquare.Piece != null && checkedSquare.Piece.Color != this.Color)
                {
                    this.IsMovable = true;
                    return;
                }
            }

            if (Position.IsInBoard(this.Position.File + 1, this.Position.Rank + (offsetPlayer * 1)))
            {
                var checkedSquare = matrix[this.Position.Rank + (offsetPlayer * 1)][this.Position.File + 1];

                if (checkedSquare.Piece != null && checkedSquare.Piece.Color != this.Color)
                {
                    this.IsMovable = true;
                    return;
                }
            }

            this.IsMovable = false;
        }

        public override void Attacking(Square[][] matrix)
        {
            int offsetPlayer = this.Color == Color.White ? -1 : 1;

            if (Position.IsInBoard(this.Position.File - 1, this.Position.Rank + (offsetPlayer * 1)))
            {
                matrix[this.Position.Rank + (offsetPlayer * 1)][this.Position.File - 1].IsAttacked.Add(this);
            }

            if (Position.IsInBoard(this.Position.File + 1, this.Position.Rank + (offsetPlayer * 1)))
            {
                matrix[this.Position.Rank + (offsetPlayer * 1)][this.Position.File + 1].IsAttacked.Add(this);
            }
        }

        public override bool Move(Position to, Square[][] matrix, int turn, Move move)
        {
            int offsetPlayer = this.Color == Color.White ? -1 : 1;

            if (!this.IsFirstMove && to.File == this.Position.File && to.Rank == this.Position.Rank + (offsetPlayer * 1))
            {
                var lastPosition = this.Color == Color.White ? 0 : 7;

                if (to.Rank == lastPosition)
                {
                    this.IsLastMove = true;
                }

                return true;
            }
            else if (this.IsFirstMove && to.File == this.Position.File &&
                (to.Rank == this.Position.Rank + (offsetPlayer * 1) || to.Rank == this.Position.Rank + (offsetPlayer * 2)))
            {
                int number = to.Rank == this.Position.Rank + (offsetPlayer * 1) ? offsetPlayer * 1 : offsetPlayer * 2;

                if (number == offsetPlayer * 2)
                {
                    move.EnPassantArgs.Turn = turn + 1;
                    move.EnPassantArgs.SquareAvailable = Factory.GetSquare(this.Position.Rank + offsetPlayer, this.Position.File);
                }

                return true;
            }

            return false;
        }

        public override bool Take(Position to, Square[][] matrix, int turn, Move move)
        {
            int offsetPlayer = this.Color == Color.White ? -1 : 1;

            if (to.Rank == this.Position.Rank + (offsetPlayer * 1) &&
                (to.File == this.Position.File - 1 || to.File == this.Position.File + 1))
            {
                var lastPosition = this.Color == Color.White ? 0 : 7;

                if (to.Rank == lastPosition)
                {
                    this.IsLastMove = true;
                }

                return true;
            }

            return false;
        }

        public override object Clone()
        {
            return new Pawn(this.Color)
            {
                Position = this.Position.Clone() as Position,
                IsFirstMove = this.IsFirstMove,
                IsMovable = this.IsMovable,
            };
        }
    }
}
