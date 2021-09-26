namespace Chess.Services.Data.Models.Pieces.Helpers
{
    using System;

    using Chess.Services.Data.Models.Pieces.Contracts;

    public class BishopBahaviour
    {
        public BishopBahaviour()
        {
        }

        public bool IsMoveAvailable(IPiece piece, Square[][] matrix)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY += 2)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX += 2)
                {
                    if (Position.IsInBoard(piece.Position.File + offsetX, piece.Position.Rank + offsetY))
                    {
                        var checkedSquare = matrix[piece.Position.Rank + offsetY][piece.Position.File + offsetX];

                        if (checkedSquare.Piece == null || checkedSquare.Piece.Color != piece.Color)
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

        public bool Move(IPiece piece, Position to, Square[][] matrix, Move move)
        {
            int filesBetween = Math.Abs(to.File - piece.Position.File);
            int ranksBetween = Math.Abs(to.Rank - piece.Position.Rank);

            if (ranksBetween == filesBetween)
            {
                if (this.Movement(to, piece.Position, matrix))
                {
                    return true;
                }
            }

            return false;
        }

        private void AttackedSquares(int offsetY, int offsetX, Square[][] matrix, IPiece piece)
        {
            for (int i = 1; i <= 7; i++)
            {
                int rank = piece.Position.Rank + (offsetY * i);
                int file = piece.Position.File + (offsetX * i);

                if (Position.IsInBoard(file, rank))
                {
                    matrix[rank][file].IsAttacked.Add(piece);

                    if (matrix[rank][file].Piece != null)
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
            int squaresCount = Math.Abs(from.Rank - to.Rank) - 1;

            if (to.Rank < from.Rank && to.File < from.File && this.OccupiedSquares(-1, -1, from, squaresCount, boardMatrix))
            {
                return true;
            }

            if (to.Rank > from.Rank && to.File > from.File && this.OccupiedSquares(1, 1, from, squaresCount, boardMatrix))
            {
                return true;
            }

            if (to.Rank < from.Rank && to.File > from.File && this.OccupiedSquares(-1, 1, from, squaresCount, boardMatrix))
            {
                return true;
            }

            if (to.Rank > from.Rank && to.File < from.File && this.OccupiedSquares(1, -1, from, squaresCount, boardMatrix))
            {
                return true;
            }

            return false;
        }

        private bool OccupiedSquares(int offsetY, int offsetX, Position from, int squaresCount, Square[][] boardMatrix)
        {
            for (int i = 1; i <= squaresCount; i++)
            {
                int file = from.File + (offsetX * i);
                int rank = from.Rank + (offsetY * i);

                if (boardMatrix[rank][file].Piece != null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
