namespace Chess.Models.Pieces
{
    using Enums;

    public class Knight : Piece
    {
        public Knight(Color color)
            : base(color)
        {
        }

        public override char Symbol => 'N';

        public override bool[,] FigureMatrix 
        { 
            get => new bool[Globals.CellRows, Globals.CellCols]
            {
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, true, true, true, false, false, false },
                { false, false, true, false, true, true, true, false, false },
                { false, true, true, true, true, true, true, false, false },
                { false, true, true, false, false, true, true, false, false },
                { false, false, false, false, true, true, true, false, false },
                { false, false, false, true, true, true, false, false, false },
                { false, false, true, true, true, true, true, false, false },
                { false, false, false, false, false, false, false, false, false }
            };
        }

        public override void IsMoveAvailable(Square[][] matrix)
        {
            if (this.MoveCheck(-1, -2, matrix) ||
                this.MoveCheck(-1, 2, matrix) ||
                this.MoveCheck(1, -2, matrix) ||
                this.MoveCheck(1, 2, matrix) ||
                this.MoveCheck(-2, -1, matrix) ||
                this.MoveCheck(-2, 1, matrix) ||
                this.MoveCheck(2, -1, matrix) ||
                this.MoveCheck(2, 1, matrix))
            {
                this.IsMovable = true;
                return;
            }

            this.IsMovable = false;
        }

        public override void Attacking(Square[][] matrix)
        {
            this.AttackedSquares(-1, -2, matrix);
            this.AttackedSquares(-1, 2, matrix);
            this.AttackedSquares(1, -2, matrix);
            this.AttackedSquares(1, 2, matrix);
            this.AttackedSquares(-2, -1, matrix);
            this.AttackedSquares(-2, 1, matrix);
            this.AttackedSquares(2, -1, matrix);
            this.AttackedSquares(2, 1, matrix);
        }

        public override bool Move(Position to, Square[][] matrix)
        {
            if (to.X == this.Position.X - 1 && to.Y == this.Position.Y - 2)
            {
                return true;
            }

            if (to.X == this.Position.X + 1 && to.Y == this.Position.Y - 2)
            {
                return true;
            }

            if (to.X == this.Position.X - 1 && to.Y == this.Position.Y + 2)
            {
                return true;
            }

            if (to.X == this.Position.X + 1 && to.Y == this.Position.Y + 2)
            {
                return true;
            }

            if (to.X == this.Position.X - 2 && to.Y == this.Position.Y - 1)
            {
                return true;
            }

            if (to.X == this.Position.X - 2 && to.Y == this.Position.Y + 1)
            {
                return true;
            }

            if (to.X == this.Position.X + 2 && to.Y == this.Position.Y - 1)
            {
                return true;
            }

            if (to.X == this.Position.X + 2 && to.Y == this.Position.Y + 1)
            {
                return true;
            }

            return false;
        }

        public override bool Take(Position to, Square[][] matrix)
        {
            return this.Move(to, matrix);
        }

        private void AttackedSquares(int y, int x, Square[][] matrix)
        {
            if (Position.IsInBoard(this.Position.X + x, this.Position.Y + y))
            {
                matrix[this.Position.Y + y][this.Position.X + x].IsAttacked.Add(this);
            }
        }

        private bool MoveCheck(int y, int x, Square[][] matrix)
        {
            if (Position.IsInBoard(this.Position.X + x, this.Position.Y + y))
            {
                var square = matrix[this.Position.Y + y][this.Position.X + x];

                if (!square.IsOccupied || square.Piece.Color != this.Color)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
