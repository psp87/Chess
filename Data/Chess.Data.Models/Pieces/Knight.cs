namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Data.Models.Enums;

    public class Knight : Piece, ICloneable
    {
        public Knight(Color color)
            : base(color)
        {
        }

        public override char Abbreviation => 'N';

        public override void IsMoveAvailable(Square[][] boardMatrix)
        {
            if (this.MoveCheck(-1, -2, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(-1, 2, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(1, -2, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(1, 2, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(-2, -1, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(-2, 1, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(2, -1, boardMatrix))
            {
                return;
            }

            if (this.MoveCheck(2, 1, boardMatrix))
            {
                return;
            }

            this.IsMoveable = false;
        }

        public override void Attacking(Square[][] boardMatrix)
        {
            this.SquareAttacked(-1, -2, boardMatrix);
            this.SquareAttacked(-1, 2, boardMatrix);
            this.SquareAttacked(1, -2, boardMatrix);
            this.SquareAttacked(1, 2, boardMatrix);
            this.SquareAttacked(-2, -1, boardMatrix);
            this.SquareAttacked(-2, 1, boardMatrix);
            this.SquareAttacked(2, -1, boardMatrix);
            this.SquareAttacked(2, 1, boardMatrix);
        }

        public override bool Move(Position toPos, Square[][] boardMatrix)
        {
            if (toPos.X == this.Position.X - 1 && toPos.Y == this.Position.Y - 2)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.X == this.Position.X + 1 && toPos.Y == this.Position.Y - 2)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.X == this.Position.X - 1 && toPos.Y == this.Position.Y + 2)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.X == this.Position.X + 1 && toPos.Y == this.Position.Y + 2)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.X == this.Position.X - 2 && toPos.Y == this.Position.Y - 1)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.X == this.Position.X - 2 && toPos.Y == this.Position.Y + 1)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.X == this.Position.X + 2 && toPos.Y == this.Position.Y - 1)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.X == this.Position.X + 2 && toPos.Y == this.Position.Y + 1)
            {
                this.Position = toPos;
                return true;
            }

            return false;
        }

        public override bool Take(Position toPos, Square[][] boardMatrix)
        {
            return this.Move(toPos, boardMatrix);
        }

        private void SquareAttacked(int x, int y, Square[][] boardMatrix)
        {
            if (Position.IsInBoard(this.Position.X + x, this.Position.Y + y))
            {
                int col = (int)this.Position.X + x;
                int row = (int)this.Position.Y + y;

                boardMatrix[col][row].IsAttacked.Add(this);
            }
        }

        private bool MoveCheck(int x, int y, Square[][] boardMatrix)
        {
            if (Position.IsInBoard(this.Position.X + x, this.Position.Y + y))
            {
                int col = (int)this.Position.X + x;
                int row = (int)this.Position.Y + y;

                var square = boardMatrix[col][row];

                if (square.Piece == null || square.Piece.Color != this.Color)
                {
                    this.IsMoveable = true;
                    return true;
                }
            }

            return false;
        }
    }
}
