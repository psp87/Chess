namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Helpers;

    public class Bishop : Piece, ICloneable
    {
        public Bishop(Color color)
            : base(color)
        {
        }

        public override char Abbreviation => 'B';

        public override void IsMoveAvailable(Square[][] boardMatrix)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                for (int k = -1; k <= 1; k += 2)
                {
                    if (Position.IsInBoard(this.Position.X + k, this.Position.Y + i))
                    {
                        int col = (int)this.Position.X + k;
                        int row = (int)this.Position.Y + i;

                        var checkedSquare = boardMatrix[col][row];

                        if (checkedSquare.Piece == null || checkedSquare.Piece.Color != this.Color)
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
            this.SquareAttacked(-1, -1, boardMatrix);
            this.SquareAttacked(-1, 1, boardMatrix);
            this.SquareAttacked(1, -1, boardMatrix);
            this.SquareAttacked(1, 1, boardMatrix);
        }

        public override bool Move(Position toPos, Square[][] boardMatrix)
        {
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

        public override bool Take(Position toPos, Square[][] boardMatrix)
        {
            return this.Move(toPos, boardMatrix);
        }

        private void SquareAttacked(int signX, int signY, Square[][] boardMatrix)
        {
            for (int i = 1; i <= 7; i++)
            {
                var newPosition = Factory.GetPosition(this.Position.X, this.Position.Y);

                newPosition.X += signX * i;
                newPosition.Y += signY * i;

                if (Position.IsInBoard(newPosition.X, newPosition.Y))
                {
                    int col = (int)newPosition.X;
                    int row = (int)newPosition.Y;

                    boardMatrix[col][row].IsAttacked.Add(this);

                    if (boardMatrix[col][row].Piece != null)
                    {
                        break;
                    }
                }
            }
        }
    }
}
