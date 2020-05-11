namespace Chess.Models.Pieces.Helpers
{
    using System;

    using Pieces.Contracts;

    public class RookBehaviour
    {
        public RookBehaviour()
        {
        }

        public bool Move(IPiece piece, Position to, Square[][] matrix)
        {
            if (to.Y != piece.Position.Y && to.X == piece.Position.X)
            {
                if (this.Movement(to, piece.Position, matrix))
                {
                    return true;
                }
            }

            if (to.Y == piece.Position.Y && to.X != piece.Position.X)
            {
                if (this.Movement(to, piece.Position, matrix))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsMoveAvailable(IPiece piece, Square[][] matrix)
        {
            if (this.MoveCheck(-1, 0, matrix, piece) ||
                this.MoveCheck(0, -1, matrix, piece) ||
                this.MoveCheck(1, 0, matrix, piece) ||
                this.MoveCheck(0, 1, matrix, piece))
            {
                return true;
            }

            return false;
        }

        public void Attacking(IPiece piece, Square[][] matrix)
        {
            this.AttackedSquaresY(-1, piece.Position.X, matrix, piece);
            this.AttackedSquaresX(piece.Position.Y, -1, matrix, piece);
            this.AttackedSquaresY(1, piece.Position.X, matrix, piece);
            this.AttackedSquaresX(piece.Position.Y, 1, matrix, piece);
        }

        private void AttackedSquaresX(int y, int signX, Square[][] matrix, IPiece piece)
        {
            for (int i = 1; i <= 7; i++)
            {
                int x = piece.Position.X + (signX * i);

                if (Position.IsInBoard(x, y))
                {
                    matrix[y][x].IsAttacked.Add(piece);

                    if (matrix[y][x].IsOccupied)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void AttackedSquaresY(int signY, int x, Square[][] matrix, IPiece piece)
        {
            for (int i = 1; i <= 7; i++)
            {
                int y = piece.Position.Y + (signY * i);

                if (Position.IsInBoard(x, y))
                {
                    matrix[y][x].IsAttacked.Add(piece);

                    if (matrix[y][x].IsOccupied)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool Movement(Position to, Position from, Square[][] matrix)
        {
            if (to.Y != from.Y)
            {
                int rowDifference = Math.Abs(from.Y - to.Y) - 1;

                for (int i = 1; i <= rowDifference; i++)
                {
                    int sign = from.Y < to.Y ? i : -i;

                    int y = from.Y + sign;

                    if (matrix[y][from.X].IsOccupied)
                    {
                        return false;
                    }
                }
            }
            else
            {
                int colDifference = Math.Abs(from.X - to.X) - 1;

                for (int i = 1; i <= colDifference; i++)
                {
                    int sign = from.X < to.X ? i : -i;

                    int x = from.X + sign;

                    if (matrix[from.Y][x].IsOccupied)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool MoveCheck(int y, int x, Square[][] matrix, IPiece piece)
        {
            if (Position.IsInBoard(piece.Position.X + x, piece.Position.Y + y))
            {
                var checkedSquare = matrix[piece.Position.Y + y][piece.Position.X + x];

                if (!checkedSquare.IsOccupied || checkedSquare.Piece.Color != piece.Color)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
