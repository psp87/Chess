namespace Chess.Services.Data.Models.Pieces
{
    using Chess.Common;
    using Chess.Common.Enums;
<<<<<<<< HEAD:Services/Chess.Services.Data/Models/Pieces/Rook.cs
    using Chess.Services.Data.Models.Pieces.Helpers;
========
<<<<<<< HEAD:Services/Chess.Services.Data/Models/Pieces/Rook.cs
    using Chess.Services.Data.Models.Pieces.Helpers;
=======
    using Chess.Data.Models.Pieces.Helpers;
    using Chess.Web.Models;
>>>>>>> master:Web/Chess.Web/Models/Pieces/Rook.cs

    public class Rook : Piece
    {
        private RookBehaviour rook;

        public Rook(Color color)
            : base(color)
        {
            this.rook = Factory.GetRookBehaviour();
        }

        public override char Symbol => Constants.RookSymbol;

        public override int Points => Constants.RookPoints;

        public override void IsMoveAvailable(Square[][] matrix)
        {
            this.IsMovable = this.rook.IsMoveAvailable(this, matrix) ? true : false;
        }

        public override void Attacking(Square[][] matrix)
        {
            this.rook.Attacking(this, matrix);
        }

        public override bool Move(Position to, Square[][] matrix, int turn, Move move)
        {
            return this.rook.Move(this, to, matrix, move);
        }

        public override bool Take(Position to, Square[][] matrix, int turn, Move move)
        {
            return this.Move(to, matrix, turn, move);
        }

        public override object Clone()
        {
            return new Rook(this.Color)
            {
                Position = this.Position.Clone() as Position,
                IsFirstMove = this.IsFirstMove,
                IsMovable = this.IsMovable,
            };
        }
    }
}
