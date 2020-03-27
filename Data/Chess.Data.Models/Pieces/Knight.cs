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
            if (toPos.PosX == this.Position.PosX - 1 && toPos.PosY == this.Position.PosY - 2)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.PosX == this.Position.PosX + 1 && toPos.PosY == this.Position.PosY - 2)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.PosX == this.Position.PosX - 1 && toPos.PosY == this.Position.PosY + 2)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.PosX == this.Position.PosX + 1 && toPos.PosY == this.Position.PosY + 2)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.PosX == this.Position.PosX - 2 && toPos.PosY == this.Position.PosY - 1)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.PosX == this.Position.PosX - 2 && toPos.PosY == this.Position.PosY + 1)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.PosX == this.Position.PosX + 2 && toPos.PosY == this.Position.PosY - 1)
            {
                this.Position = toPos;
                return true;
            }

            if (toPos.PosX == this.Position.PosX + 2 && toPos.PosY == this.Position.PosY + 1)
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
            var newPosition = Factory.GetPosition(this.Position.PosX + x, this.Position.PosY + y);

            if (newPosition.IsInBoard())
            {
                int col = (int)newPosition.PosX;
                int row = (int)newPosition.PosY;

                boardMatrix[col][row].IsAttacked.Add(this);
            }
        }

        private void MoveCheck(int x, int y, Square[][] boardMatrix)
        {
            var newPosition = Factory.GetPosition(this.Position.PosX + x, this.Position.PosY + y);

            if (newPosition.IsInBoard())
            {
                int col = (int)newPosition.PosX;
                int row = (int)newPosition.PosY;

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
