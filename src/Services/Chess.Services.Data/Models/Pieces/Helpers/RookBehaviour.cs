namespace Chess.Services.Data.Models.Pieces.Helpers
{
    using System;

    using Chess.Services.Data.Models.Pieces.Contracts;

    public class RookBehaviour
    {
        public RookBehaviour()
        {
        }

        public bool Move(IPiece piece, Position to, Square[][] matrix, Move move)
        {
            if (to.Rank != piece.Position.Rank && to.File == piece.Position.File)
            {
                if (this.Movement(to, piece.Position, matrix))
                {
                    return true;
                }
            }

            if (to.Rank == piece.Position.Rank && to.File != piece.Position.File)
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
            this.AttackedSquaresY(-1, piece.Position.File, matrix, piece);
            this.AttackedSquaresX(piece.Position.Rank, -1, matrix, piece);
            this.AttackedSquaresY(1, piece.Position.File, matrix, piece);
            this.AttackedSquaresX(piece.Position.Rank, 1, matrix, piece);
        }

        private void AttackedSquaresX(int rank, int offsetX, Square[][] matrix, IPiece piece)
        {
            for (int i = 1; i <= 7; i++)
            {
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

        private void AttackedSquaresY(int offsetY, int file, Square[][] matrix, IPiece piece)
        {
            for (int i = 1; i <= 7; i++)
            {
                int rank = piece.Position.Rank + (offsetY * i);

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

        private bool Movement(Position to, Position from, Square[][] matrix)
        {
            if (to.Rank != from.Rank)
            {
                int ranksBetween = Math.Abs(from.Rank - to.Rank) - 1;

                for (int i = 1; i <= ranksBetween; i++)
                {
                    int offsetY = from.Rank < to.Rank ? i : -i;

                    int rank = from.Rank + offsetY;

                    if (matrix[rank][from.File].Piece != null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                int filesBetween = Math.Abs(from.File - to.File) - 1;

                for (int i = 1; i <= filesBetween; i++)
                {
                    int offsetX = from.File < to.File ? i : -i;

                    int file = from.File + offsetX;

                    if (matrix[from.Rank][file].Piece != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool MoveCheck(int rank, int file, Square[][] matrix, IPiece piece)
        {
            if (Position.IsInBoard(piece.Position.File + file, piece.Position.Rank + rank))
            {
                var checkedSquare = matrix[piece.Position.Rank + rank][piece.Position.File + file];

                if (checkedSquare.Piece == null || checkedSquare.Piece.Color != piece.Color)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
