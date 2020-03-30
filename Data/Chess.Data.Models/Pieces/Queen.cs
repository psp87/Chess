namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Helpers;

    public class Queen : Piece, ICloneable
    {
        public Queen(Color color)
            : base(color)
        {
        }

        public override char Abbreviation => 'Q';

        public override void IsMoveAvailable(Square[][] boardMatrix)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (i == 0 && k == 0)
                    {
                        continue;
                    }

                    if (Position.IsInBoard(this.Position.X + k, this.Position.Y + i))
                    {
                        var checkedSquare = boardMatrix[(int)this.Position.X + k][(int)this.Position.Y + i];

                        if (checkedSquare.Piece == null || checkedSquare.Piece.Color != this.Color)
                        {
                            this.IsMoveable = true;
                            return;
                        }
                    }
                }
            }

            this.IsMoveable = false;
        }

        public override void Attacking(Square[][] boardMatrix)
        {
            this.SquareAttacked(-1, -1, boardMatrix);
            this.SquareAttacked(-1, 0, boardMatrix);
            this.SquareAttacked(-1, 1, boardMatrix);
            this.SquareAttacked(0, -1, boardMatrix);
            this.SquareAttacked(0, 1, boardMatrix);
            this.SquareAttacked(1, -1, boardMatrix);
            this.SquareAttacked(1, 0, boardMatrix);
            this.SquareAttacked(1, 1, boardMatrix);
        }

        public override bool Move(Position toPos, Square[][] boardMatrix)
        {
            if (toPos.Y != this.Position.Y && toPos.X == this.Position.X)
            {
                if (RookChecks.Movement(toPos, this.Position, boardMatrix))
                {
                    this.Position.Y = toPos.Y;
                    return true;
                }
            }

            if (toPos.Y == this.Position.Y && toPos.X != this.Position.X)
            {
                if (RookChecks.Movement(toPos, this.Position, boardMatrix))
                {
                    this.Position.X = toPos.X;
                    return true;
                }
            }

            int differenceX = Math.Abs(toPos.X - this.Position.X);
            int differenceY = Math.Abs(toPos.Y - this.Position.Y);

            if (differenceY == differenceX)
            {
                if (BishopChecks.Movement(toPos, this.Position, boardMatrix))
                {
                    this.Position.X = toPos.X;
                    this.Position.Y = toPos.Y;
                    return true;
                }
            }

            return false;
        }

        public override bool Take(Position toPosition, Square[][] boardMatrix)
        {
            return this.Move(toPosition, boardMatrix);
        }

        private void SquareAttacked(int signX, int signY, Square[][] boardMatrix)
        {
            for (int i = 1; i <= 7; i++)
            {
                for (int k = 1; k <= 7; k++)
                {
                    var newPosition = Factory.GetPosition(this.Position.X, this.Position.Y);

                    newPosition.X += signX * k;
                    newPosition.Y += signY * i;

                    if (Position.IsInBoard(newPosition.X, newPosition.Y))
                    {
                        boardMatrix[(int)newPosition.X][(int)newPosition.Y].IsAttacked.Add(this);

                        if (boardMatrix[(int)newPosition.X][(int)newPosition.Y].Piece != null)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
