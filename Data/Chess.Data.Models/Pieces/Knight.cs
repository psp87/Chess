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
            this.MoveCheck(-1, -2, boardMatrix);
            this.MoveCheck(-1, 2, boardMatrix);
            this.MoveCheck(1, -2, boardMatrix);
            this.MoveCheck(1, 2, boardMatrix);
            this.MoveCheck(-2, -1, boardMatrix);
            this.MoveCheck(-2, 1, boardMatrix);
            this.MoveCheck(2, -1, boardMatrix);
            this.MoveCheck(2, 1, boardMatrix);
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
            var newPosition = Factory.GetPosition(this.Position.X + x, this.Position.Y + y);

            if (newPosition.IsInBoard())
            {
                int col = (int)newPosition.X;
                int row = (int)newPosition.Y;

                boardMatrix[col][row].IsAttacked.Add(this);
            }
        }

        private void MoveCheck(int x, int y, Square[][] boardMatrix)
        {
            var newPosition = Factory.GetPosition(this.Position.X + x, this.Position.Y + y);

            if (newPosition.IsInBoard())
            {
                int col = (int)newPosition.X;
                int row = (int)newPosition.Y;

                var square = boardMatrix[col][row];

                if (square.Piece == null || square.Piece.Color != this.Color)
                {
                    this.IsMoveable = true;
                }
            }

            this.IsMoveable = false;
        }
    }
}
