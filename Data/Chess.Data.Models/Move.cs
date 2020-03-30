namespace Chess.Data.Models
{
    using System;
    using System.Linq;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces;

    public class Move : ICloneable
    {
        private Board board;
        private MoveType moveType;

        public Move(Square start, Square end, Board board, MoveType moveType)
        {
            this.Start = start;
            this.End = end;
            this.board = board;
            this.moveType = moveType;
        }

        public Move()
        {
        }

        public event EventHandler OnPieceCaputured;

        public Square Start { get; set; }

        public Square End { get; set; }

        public string Do()
        {
            Piece lostPiece = null;

            if (this.moveType == MoveType.EnPassant)
            {
                this.board.ShiftPiece(this.Start, this.End);
                var square = this.End.Color == Color.White ?
                            this.board.GetSquareAtPosition(this.End.Position + new Position(0, 1)) :
                            this.board.GetSquareAtPosition(this.End.Position + new Position(0, -1));

                this.OnPieceCaputured?.Invoke(square.Piece, new EventArgs());
                lostPiece = square.Piece;
                square.Piece = null;
            }

            if (this.moveType == MoveType.Castle)
            {
                var dir = this.Start.Position.GetDirection(this.End.Position);
                var rooks = board.GetAllSquaresWithPieces(this.Start.Piece.Color).Where(x => x.Piece is Rook);
                this.board.ShiftPiece(this.Start, this.End);
                this.board.ShiftPiece(rooks.FirstOrDefault(x => End.Position.IsInDirection(dir, x.Position)),
                    this.board.GetSquareAtPosition(End.Position + new Position(dir.PosX * -1, dir.PosY)));
            }

            if (this.moveType == MoveType.Normal)
            {
                lostPiece = this.board.ShiftPiece(this.Start, this.End);
                if (lostPiece != null)
                {
                    this.OnPieceCaputured?.Invoke(lostPiece, new EventArgs());
                }
            }

            return lostPiece?.GetType().Name;
        }

        public override string ToString()
        {
            return this.Start.ToString() + "->" + this.End.ToString();
        }

        public object Clone()
        {
            return new Move()
            {
                Start = this.Start.Clone() as Square,
                End = this.End.Clone() as Square,
            };
        }
    }