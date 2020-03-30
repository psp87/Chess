namespace Chess.Data.Models.Pieces
{
    using System;
    using System.Linq;

    using Chess.Data.Models.Enums;

    public class King : Piece, ICloneable
    {
        public King(Color color)
            : base(color)
        {
        }

        public override char Abbreviation => 'K';

        public override void IsMoveAvailable(Square[][] boardMatrix)
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
                        var checkedSquare = boardMatrix[(int)this.Position.X + x][(int)this.Position.Y + y];

                        if ((checkedSquare.Piece != null &&
                            checkedSquare.Piece.Color != this.Color &&
                            !checkedSquare.IsAttacked.Where(x => x.Color != this.Color).Any()) ||
                            (checkedSquare.Piece == null &&
                            !checkedSquare.IsAttacked.Where(x => x.Color != this.Color).Any()))
                        {
                            this.IsMoveable = true;
                        }
                    }
                }
            }

            this.IsMoveable = false;
        }

        public override void Attacking(Square[][] boardMatrix)
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
                        var posX = (int)this.Position.X + x;
                        var posY = (int)this.Position.Y + y;

                        boardMatrix[posX][posY].IsAttacked.Add(this);
                    }
                }
            }
        }

        public override bool Move(Position toPos, Square[][] boardMatrix)
        {
            if (!boardMatrix[(int)toPos.X][(int)toPos.Y].IsAttacked.Where(x => x.Color != this.Color).Any())
            {
                for (int posX = -1; posX <= 1; posX++)
                {
                    for (int posY = -1; posY <= 1; posY++)
                    {
                        if (posX == 0 && posY == 0)
                        {
                            continue;
                        }

                        if (toPos.X == this.Position.X + posX && toPos.Y == this.Position.Y + posY)
                        {
                            this.Position.Y += posX;
                            this.Position.X += posY;
                            this.IsFirstMove = false;
                            return true;
                        }
                    }
                }

                if (this.IsFirstMove && toPos.Y == this.Position.Y)
                {
                    if (toPos.X == this.Position.X + 2)
                    {
                        var square = boardMatrix[7][(int)this.Position.Y];
                        if (this.OccupiedSquaresCheck(toPos, boardMatrix) && square.Piece is Rook && square.Piece.IsFirstMove)
                        {
                            this.Position.X += 2;
                            this.IsFirstMove = false;

                            // TO DO CASTLE LOGIC
                            return true;
                        }
                    }

                    if (toPos.X == this.Position.X - 2)
                    {
                        var square = boardMatrix[0][(int)this.Position.Y];
                        if (this.OccupiedSquaresCheck(toPos, boardMatrix) && square.Piece is Rook && square.Piece.IsFirstMove)
                        {
                            this.Position.X -= 2;
                            this.IsFirstMove = false;

                            // TO DO CASTLE LOGIC
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override bool Take(Position toPos, Square[][] boardMatrix)
        {
            return this.Move(toPos, boardMatrix);
        }

        private bool OccupiedSquaresCheck(Position toPos, Square[][] boardMatrix)
        {
            int colDifference = Math.Abs((int)this.Position.X - (int)toPos.X) - 1;

            if ((int)this.Position.X > (int)toPos.X)
            {
                colDifference += 2;
            }

            for (int i = 1; i <= colDifference; i++)
            {
                int sign = this.Position.X < toPos.X ? i : -i;

                int colCheck = (int)this.Position.X + sign;

                if (boardMatrix[(int)this.Position.Y][colCheck].Piece != null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
