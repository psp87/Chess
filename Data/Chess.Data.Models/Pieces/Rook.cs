namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Helpers;

    public class Rook : Piece, ICloneable
    {
        public Rook(Color color)
            : base(color)
        {
        }

        public override char Abbreviation => 'R';

        public override void IsMoveAvailable(Square[][] boardMatrix)
        {
            if (this.MoveCheck(-1, -1, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(-1, 1, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(1, -1, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(1, 1, boardMatrix))
            {
                return;
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

            return false;
        }

        public override bool Take(Position toPos, Square[][] boardMatrix)
        {
            return this.Move(toPos, boardMatrix);
        }

        private bool MoveCheck(int x, int y, Square[][] boardMatrix)
        {
            if (Position.IsInBoard(this.Position.X + x, this.Position.Y + y))
            {
                int col = (int)this.Position.X + x;
                int row = (int)this.Position.Y + y;

                var checkedSquare = boardMatrix[col][row];

                if (checkedSquare.Piece == null || checkedSquare.Piece.Color != this.Color)
                {
                    this.IsMoveable = true;
                    return true;
                }
            }

            return false;
        }

        private void SquareAttacked(int signX, int signY, Square[][] boardMatrix)
        {
            var newPosition = Factory.GetPosition(this.Position.X, this.Position.Y);

            for (int i = 1; i <= 7; i++)
            {
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
