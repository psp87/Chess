namespace Chess.Models.Pieces
{
    using Enums;
    using Helpers;

    public class Pawn : Piece
    {
        public Pawn(Color color)
            : base(color)
        {
        }

        public override char Symbol => 'P';

        public override bool[,] FigureMatrix 
        { 
            get => new bool[Globals.CellRows, Globals.CellCols]
            {
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, true, false, false, false, false },
                { false, false, false, true, true, true, false, false, false },
                { false, false, false, true, true, true, false, false, false },
                { false, false, false, false, true, false, false, false, false },
                { false, false, false, true, true, true, false, false, false },
                { false, false, true, true, true, true, true, false, false },
                { false, false, false, false, false, false, false, false, false }
            };
        }

        public override void IsMoveAvailable(Square[][] matrix)
        {
            int sign = this.Color == Color.Light ? -1 : 1;

            if (Position.IsInBoard(this.Position.X, this.Position.Y + (sign * 1)))
            {
                var checkedSquare = matrix[this.Position.Y + (sign * 1)][this.Position.X];

                if (!checkedSquare.IsOccupied)
                {
                    this.IsMovable = true;
                    return;
                }
            }

            if (Position.IsInBoard(this.Position.X - 1, this.Position.Y + (sign * 1)))
            {
                var checkedSquare = matrix[this.Position.Y + (sign * 1)][this.Position.X - 1];

                if (checkedSquare.IsOccupied && checkedSquare.Piece.Color != this.Color)
                {
                    this.IsMovable = true;
                    return;
                }
            }

            if (Position.IsInBoard(this.Position.X + 1, this.Position.Y + (sign * 1)))
            {
                var checkedSquare = matrix[this.Position.Y + (sign * 1)][this.Position.X + 1];

                if (checkedSquare.IsOccupied && checkedSquare.Piece.Color != this.Color)
                {
                    this.IsMovable = true;
                    return;
                }
            }

            this.IsMovable = false;
        }

        public override void Attacking(Square[][] matrix)
        {
            int sign = this.Color == Color.Light ? -1 : 1;

            if (Position.IsInBoard(this.Position.X - 1, this.Position.Y + (sign * 1)))
            {
                matrix[this.Position.Y + (sign * 1)][this.Position.X - 1].IsAttacked.Add(this);
            }

            if (Position.IsInBoard(this.Position.X + 1, this.Position.Y + (sign * 1)))
            {
                matrix[this.Position.Y + (sign * 1)][this.Position.X + 1].IsAttacked.Add(this);
            }
        }

        public override bool Move(Position to, Square[][] matrix)
        {
            int sign = this.Color == Color.Light ? -1 : 1;

            if (!this.IsFirstMove && to.X == this.Position.X && to.Y == this.Position.Y + (sign * 1))
            {
                var lastPosition = this.Color == Color.Light ? 0 : 7;

                if (to.Y == lastPosition)
                {
                    this.IsLastMove = true;
                }

                return true;
            }
            else if (this.IsFirstMove && to.X == this.Position.X &&
                (to.Y == this.Position.Y + (sign * 1) || to.Y == this.Position.Y + (sign * 2)))
            {
                int number = to.Y == this.Position.Y + (sign * 1) ? sign * 1 : sign * 2;

                this.IsFirstMove = false;

                if (number == sign * 2)
                {
                    EnPassant.Turn = Globals.TurnCounter + 1;
                    EnPassant.Position = Factory.GetPosition(this.Position.Y + (sign * 1), this.Position.X);
                }

                return true;
            }

            return false;
        }

        public override bool Take(Position to, Square[][] matrix)
        {
            int sign = this.Color == Color.Light ? -1 : 1;

            if (to.Y == this.Position.Y + (sign * 1) &&
                (to.X == this.Position.X - 1 || to.X == this.Position.X + 1))
            {
                var lastPosition = this.Color == Color.Light ? 0 : 7;

                if (to.Y == lastPosition)
                {
                    this.IsLastMove = true;
                }

                return true;
            }

            return false;
        }
    }
}
