namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;

    public class Rook : Piece, ICloneable
    {
        public Rook(Color color)
            : base(color)
        {
        }

        public override char Abbreviation => 'R';

        public override void IsMoveAvailable(Square[][] boardMatrix)
        {
            this.MoveCheck(-1, -1, boardMatrix);
            this.MoveCheck(-1, 1, boardMatrix);
            this.MoveCheck(1, -1, boardMatrix);
            this.MoveCheck(1, 1, boardMatrix);
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
            if (toPos.PosY != this.Position.PosY && toPos.PosX == this.Position.PosX)
            {
                if (this.OccupiedSquaresCheck(toPos, boardMatrix))
                {
                    this.Position.PosY = toPos.PosY;
                    return true;
                }
            }

            if (toPos.PosY == this.Position.PosY && toPos.PosX != this.Position.PosX)
            {
                if (this.OccupiedSquaresCheck(toPos, boardMatrix))
                {
                    this.Position.PosX = toPos.PosX;
                    return true;
                }
            }

            return false;
        }

        public override bool Take(Position toPos, Square[][] boardMatrix)
        {
            return this.Move(toPos, boardMatrix);
        }

        private void MoveCheck(int x, int y, Square[][] boardMatrix)
        {
            var newPosition = Factory.GetPosition(this.Position.PosX, this.Position.PosY);

            newPosition.PosX += x;
            newPosition.PosY += y;

            if (newPosition.IsInBoard())
            {
                int col = (int)newPosition.PosX;
                int row = (int)newPosition.PosY;

                var checkedSquare = boardMatrix[col][row];

                if (checkedSquare.Piece == null || checkedSquare.Piece.Color != this.Color)
                {
                    this.IsMoveable = true;
                }
            }

            this.IsMoveable = false;
        }

        private void SquareAttacked(int signX, int signY, Square[][] boardMatrix)
        {
            var newPosition = Factory.GetPosition(this.Position.PosX, this.Position.PosY);

            for (int i = 1; i <= 7; i++)
            {
                newPosition.PosX += signX * i;
                newPosition.PosY += signY * i;

                if (newPosition.IsInBoard())
                {
                    int col = (int)newPosition.PosX;
                    int row = (int)newPosition.PosY;

                    boardMatrix[col][row].IsAttacked.Add(this);

                    if (boardMatrix[col][row].Piece != null)
                    {
                        break;
                    }
                }
            }
        }

        private bool OccupiedSquaresCheck(Position toPos, Square[][] boardMatrix)
        {
            if (toPos.PosY != this.Position.PosY)
            {
                int rowDifference = Math.Abs((int)this.Position.PosY - (int)this.Position.PosY) - 1;

                for (int i = 1; i <= rowDifference; i++)
                {
                    int sign = this.Position.PosY < toPos.PosY ? i : -i;

                    int rowCheck = (int)this.Position.PosY + sign;

                    if (boardMatrix[rowCheck][(int)this.Position.PosX].Piece != null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                int colDifference = Math.Abs((int)this.Position.PosX - (int)toPos.PosX) - 1;

                for (int i = 1; i <= colDifference; i++)
                {
                    int sign = this.Position.PosX < toPos.PosX ? i : -i;

                    int colCheck = (int)this.Position.PosX + sign;

                    if (boardMatrix[(int)this.Position.PosY][colCheck].Piece != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
