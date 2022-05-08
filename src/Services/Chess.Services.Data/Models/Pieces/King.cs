namespace Chess.Services.Data.Models.Pieces
{
    using System;
    using System.Linq;

    using Chess.Common.Constants;
    using Chess.Common.Enums;

    public class King : Piece
    {
        public King(Color color)
            : base(color)
        {
        }

        public override char Symbol => SymbolConstants.King;

        public override int Points => default;

        public override void IsMoveAvailable(Square[][] matrix)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {
                    if (offsetX == 0 && offsetY == 0)
                    {
                        continue;
                    }

                    if (Position.IsInBoard(this.Position.File + offsetX, this.Position.Rank + offsetY))
                    {
                        var checkedSquare = matrix[this.Position.Rank + offsetY][this.Position.File + offsetX];

                        if ((checkedSquare.Piece != null &&
                            checkedSquare.Piece.Color != this.Color &&
                            !checkedSquare.IsAttacked.Where(p => p.Color != this.Color).Any()) ||
                            (checkedSquare.Piece == null &&
                            !checkedSquare.IsAttacked.Where(p => p.Color != this.Color).Any()))
                        {
                            this.IsMovable = true;
                            return;
                        }
                    }
                }
            }

            this.IsMovable = false;
        }

        public override void Attacking(Square[][] matrix)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {
                    if (offsetX == 0 && offsetY == 0)
                    {
                        continue;
                    }

                    if (Position.IsInBoard(this.Position.File + offsetX, this.Position.Rank + offsetY))
                    {
                        matrix[this.Position.Rank + offsetY][this.Position.File + offsetX].IsAttacked.Add(this);
                    }
                }
            }
        }

        public override bool Move(Position to, Square[][] matrix, int turn, Move move)
        {
            if (!matrix[to.Rank][to.File].IsAttacked.Where(x => x.Color != this.Color).Any())
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        if (offsetX == 0 && offsetY == 0)
                        {
                            continue;
                        }

                        if (to.File == this.Position.File + offsetX && to.Rank == this.Position.Rank + offsetY)
                        {
                            return true;
                        }
                    }
                }

                if (this.IsFirstMove && to.Rank == this.Position.Rank &&
                    (to.File == this.Position.File + 2 || to.File == this.Position.File - 2))
                {
                    int sign = to.File == this.Position.File + 2 ? -1 : 1;
                    int lastPiecePosition = to.File == this.Position.File + 2 ? 7 : 0;

                    var firstSquareOnWay = matrix[this.Position.Rank][to.File + sign];
                    var secondSquareOnWay = matrix[this.Position.Rank][to.File];
                    var lastPiece = matrix[this.Position.Rank][lastPiecePosition].Piece;

                    if (this.OccupiedSquaresCheck(to, matrix) &&
                        lastPiece != null &&
                        lastPiece.IsType(SymbolConstants.Rook) &&
                        lastPiece.IsFirstMove &&
                        !firstSquareOnWay.IsAttacked.Where(x => x.Color != this.Color).Any() &&
                        !secondSquareOnWay.IsAttacked.Where(x => x.Color != this.Color).Any())
                    {
                        matrix[this.Position.Rank][to.File + sign].Piece = matrix[this.Position.Rank][lastPiecePosition].Piece;
                        matrix[this.Position.Rank][lastPiecePosition].Piece = null;

                        move.Type = MoveType.Castling;
                        move.CastlingArgs.IsCastlingMove = true;
                        move.CastlingArgs.RookSource = matrix[this.Position.Rank][lastPiecePosition].ToString();
                        move.CastlingArgs.RookTarget = matrix[this.Position.Rank][to.File + sign].ToString();
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool Take(Position to, Square[][] matrix, int turn, Move move)
        {
            return this.Move(to, matrix, turn, move);
        }

        public override object Clone()
        {
            return new King(this.Color)
            {
                Position = this.Position.Clone() as Position,
                IsFirstMove = this.IsFirstMove,
                IsMovable = this.IsMovable,
            };
        }

        private bool OccupiedSquaresCheck(Position to, Square[][] matrix)
        {
            int filesBetween = Math.Abs(this.Position.File - to.File) - 1;

            if (this.Position.File > to.File)
            {
                filesBetween += 2;
            }

            for (int i = 1; i <= filesBetween; i++)
            {
                int sign = this.Position.File < to.File ? i : -i;

                if (matrix[this.Position.Rank][this.Position.File + sign].Piece != null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
