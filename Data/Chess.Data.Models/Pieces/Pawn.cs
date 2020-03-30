namespace Chess.Data.Models.Pieces
{
    using System;

    using Chess.Common;
    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces.Contracts;

    public class Pawn : Piece, ICloneable
    {
        public Pawn(Color color)
            : base(color)
        {
        }

        public override char Abbreviation => 'P';

        public override void IsMoveAvailable(Square[][] boardMatrix)
        {
            int sign = this.Color == Color.Light ? 1 : -1;

            if (Position.IsInBoard(this.Position.X, this.Position.Y + (sign * 1)))
            {
                var checkedSquare = boardMatrix[(int)this.Position.X][(int)this.Position.Y + (sign * 1)];

                if (checkedSquare.Piece == null)
                {
                    this.IsMoveable = true;
                    return;
                }
            }

            if (Position.IsInBoard(this.Position.X - 1, this.Position.Y + (sign * 1)))
            {
                var checkedSquare = boardMatrix[(int)this.Position.X - 1][(int)this.Position.Y + (sign * 1)];

                if (checkedSquare.Piece != null && checkedSquare.Piece.Color != this.Color)
                {
                    this.IsMoveable = true;
                    return;
                }
            }

            if (Position.IsInBoard(this.Position.X + 1, this.Position.Y + (sign * 1)))
            {
                var checkedSquare = boardMatrix[(int)this.Position.X + 1][(int)this.Position.Y + (sign * 1)];

                if (checkedSquare.Piece != null && checkedSquare.Piece.Color != this.Color)
                {
                    this.IsMoveable = true;
                    return;
                }
            }

            this.IsMoveable = false;
        }

        public override void Attacking(Square[][] boardMatrix)
        {
            int sign = this.Color == Color.Light ? 1 : -1;

            if (Position.IsInBoard(this.Position.X - 1, this.Position.Y + (sign * 1)))
            {
                boardMatrix[(int)this.Position.X - 1][(int)this.Position.Y + (sign * 1)].IsAttacked.Add(this);
            }

            if (Position.IsInBoard(this.Position.X + 1, this.Position.Y + (sign * 1)))
            {
                boardMatrix[(int)this.Position.X + 1][(int)this.Position.Y + (sign * 1)].IsAttacked.Add(this);
            }
        }

        public override bool Move(Position toPos, Square[][] boardMatrix)
        {
            int sign = this.Color == Color.Light ? 1 : -1;

            if (!this.IsFirstMove && toPos.X == this.Position.X && toPos.Y == this.Position.Y + (sign * 1))
            {
                var lastPosition = this.Color == Color.Light ? Y.Eight : Y.One;

                if (toPos.Y == lastPosition)
                {
                    this.IsLastMove = true;
                }

                this.Position.Y += sign * 1;
                return true;
            }
            else if (this.IsFirstMove && toPos.X == this.Position.X &&
                (toPos.Y == this.Position.Y + (sign * 1) || toPos.Y == this.Position.Y + (sign * 2)))
            {
                int number = toPos.Y == this.Position.Y + (sign * 1) ? sign * 1 : sign * 2;
                this.Position.Y += number;

                this.IsFirstMove = false;

                if (number == sign * 2)
                {
                    EnPassant.Turn = GlobalConstants.TurnCounter;
                    EnPassant.Position = Factory.GetPosition(this.Position.X, this.Position.Y + (sign * 1));
                }

                return true;
            }

            return false;
        }

        public override bool Take(Position toPos, Square[][] boardMatrix)
        {
            int sign = this.Color == Color.Light ? 1 : -1;

            if (toPos.Y == this.Position.Y + (sign * 1) &&
                (toPos.X == this.Position.X - 1 || toPos.X == this.Position.X + 1))
            {
                var lastPosition = this.Color == Color.Light ? Y.Eight : Y.One;

                if (toPos.Y == lastPosition)
                {
                    this.IsLastMove = true;
                }

                int number = toPos.X == this.Position.X - 1 ? -1 : 1;
                this.Position.X += number;
                this.Position.Y += sign * 1;
            }

            return false;
        }

        public IPiece Promotion(IPiece chosenPiece)
        {
            if (chosenPiece is Queen)
            {
                return Factory.GetQueen(this.Color);
            }

            if (chosenPiece is Rook)
            {
                return Factory.GetRook(this.Color);
            }

            if (chosenPiece is Bishop)
            {
                return Factory.GetBishop(this.Color);
            }

            if (chosenPiece is Knight)
            {
                return Factory.GetKnight(this.Color);
            }

            return null;
        }
    }
}
