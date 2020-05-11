namespace Chess.Models.Pieces.Helpers
{
    using System;

    using Pieces.Contracts;

    public class BishopBahaviour
    {
        public BishopBahaviour()
        {
        }

        public bool IsMoveAvailable(IPiece piece, Square[][] matrix)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                for (int k = -1; k <= 1; k += 2)
                {
                    if (Position.IsInBoard(piece.Position.X + k, piece.Position.Y + i))
                    {
                        var checkedSquare = matrix[piece.Position.Y + i][piece.Position.X + k];

                        if (checkedSquare.IsOccupied || checkedSquare.Piece.Color != piece.Color)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Attacking(IPiece piece, Square[][] matrix)
        {
            this.AttackedSquares(-1, -1, matrix, piece);
            this.AttackedSquares(-1, 1, matrix, piece);
            this.AttackedSquares(1, -1, matrix, piece);
            this.AttackedSquares(1, 1, matrix, piece);
        }

        public bool Move(IPiece piece, Position to, Square[][] matrix)
        {
            int differenceX = Math.Abs(to.X - piece.Position.X);
            int differenceY = Math.Abs(to.Y - piece.Position.Y);

            if (differenceY == differenceX)
            {
                if (this.Movement(to, piece.Position, matrix))
                {
                    return true;
                }
            }

            return false;
        }

        private void AttackedSquares(int signY, int signX, Square[][] matrix, IPiece piece)
        {
            for (int i = 1; i <= 7; i++)
            {
                int y = piece.Position.Y + (signY * i);
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

        private bool Movement(Position to, Position from, Square[][] boardMatrix)
        {
            int squaresCount = Math.Abs(from.Y - to.Y) - 1;

            if (to.Y < from.Y && to.X < from.X && this.OccupiedSquares(-1, -1, from, squaresCount, boardMatrix))
            {
                return true;
            }

            if (to.Y > from.Y && to.X > from.X && this.OccupiedSquares(1, 1, from, squaresCount, boardMatrix))
            {
                return true;
            }

            if (to.Y < from.Y && to.X > from.X && this.OccupiedSquares(-1, 1, from, squaresCount, boardMatrix))
            {
                return true;
            }

            if (to.Y > from.Y && to.X < from.X && this.OccupiedSquares(1, -1, from, squaresCount, boardMatrix))
            {
                return true;
            }

            return false;
        }

        private bool OccupiedSquares(int signY, int signX, Position from, int squaresCount, Square[][] boardMatrix)
        {
            for (int i = 1; i <= squaresCount; i++)
            {
                int x = from.X + (signX * i);
                int y = from.Y + (signY * i);

                if (boardMatrix[y][x].IsOccupied)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
