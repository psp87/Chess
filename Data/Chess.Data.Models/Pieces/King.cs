namespace Chess.Models.Pieces
{
    using System;
    using System.Linq;

    using Enums;
    using View;

    public class King : Piece
    {
        private Draw drawer = Factory.GetDraw();

        public King(Color color)
            : base(color)
        {
        }

        public override char Symbol => 'K';

        public override bool[,] FigureMatrix
        {
            get => new bool[Globals.CellRows, Globals.CellCols]
            {
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, true, false, false, false, false },
                { false, false, false, true, true, true, false, false, false },
                { false, true, true, false, true, false, true, true, false },
                { false, true, true, true, false, true, true, true, false },
                { false, true, true, true, true, true, true, true, false },
                { false, false, true, true, true, true, true, false, false },
                { false, false, true, true, true, true, true, false, false },
                { false, false, false, false, false, false, false, false, false }
            };
        }

        public override void IsMoveAvailable(Square[][] matrix)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    if (Position.IsInBoard(this.Position.X + x, this.Position.Y + y))
                    {
                        var checkedSquare = matrix[this.Position.Y + y][this.Position.X + x];

                        if ((checkedSquare.IsOccupied &&
                            checkedSquare.Piece.Color != this.Color &&
                            !checkedSquare.IsAttacked.Where(p => p.Color != this.Color).Any()) ||
                            (!checkedSquare.IsOccupied &&
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
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    if (Position.IsInBoard(this.Position.X + x, this.Position.Y + y))
                    {
                        matrix[this.Position.Y + y][this.Position.X + x].IsAttacked.Add(this);
                    }
                }
            }
        }

        public override bool Move(Position to, Square[][] matrix)
        {
            if (!matrix[to.Y][to.X].IsAttacked.Where(x => x.Color != this.Color).Any())
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        if (to.X == this.Position.X + x && to.Y == this.Position.Y + y)
                        {
                            this.IsFirstMove = false;
                            return true;
                        }
                    }
                }

                if (this.IsFirstMove && to.Y == this.Position.Y && 
                    (to.X == this.Position.X + 2 || to.X == this.Position.X - 2))
                {
                    int sign = to.X == this.Position.X + 2 ? -1 : 1;
                    int lastPiecePosition = to.X == this.Position.X + 2 ? 7 : 0;

                    var firstSquareOnWay = matrix[this.Position.Y][to.X + sign];
                    var secondSquareOnWay = matrix[this.Position.Y][to.X];
                    var lastPiece = matrix[this.Position.Y][lastPiecePosition].Piece;

                    if (this.OccupiedSquaresCheck(to, matrix) && 
                        lastPiece is Rook && 
                        lastPiece.IsFirstMove && 
                        !firstSquareOnWay.IsAttacked.Where(x => x.Color != this.Color).Any() && 
                        !secondSquareOnWay.IsAttacked.Where(x => x.Color != this.Color).Any())
                    {
                        this.IsFirstMove = false;

                        matrix[this.Position.Y][to.X + sign].Piece = matrix[this.Position.Y][lastPiecePosition].Piece;
                        matrix[this.Position.Y][lastPiecePosition].Piece = Factory.GetEmpty();

                        this.drawer.EmptySquare(this.Position.Y, lastPiecePosition);
                        this.drawer.Piece(this.Position.Y, to.X + sign, matrix[this.Position.Y][to.X + sign].Piece);

                        return true;
                    }
                }
            }

            return false;
        }

        public override bool Take(Position to, Square[][] matrix)
        {
            return this.Move(to, matrix);
        }

        private bool OccupiedSquaresCheck(Position to, Square[][] matrix)
        {
            int colDifference = Math.Abs(this.Position.X - to.X) - 1;

            if (this.Position.X > to.X)
            {
                colDifference += 2;
            }

            for (int i = 1; i <= colDifference; i++)
            {
                int sign = this.Position.X < to.X ? i : -i;

                if (matrix[this.Position.Y][this.Position.X + sign].IsOccupied)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
