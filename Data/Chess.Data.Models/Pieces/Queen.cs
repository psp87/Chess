namespace Chess.Models.Pieces
{
    using Enums;
    using Helpers;

    public class Queen : Piece
    {
        private RookBehaviour rook;
        private BishopBahaviour bishop;

        public Queen(Color color)
            : base(color)
        {
            this.rook = Factory.GetRookBehaviour();
            this.bishop = Factory.GetBishopBehaviour();
        }

        public override char Symbol => 'Q';

        public override bool[,] FigureMatrix 
        { 
            get => new bool[Globals.CellRows, Globals.CellCols]
            {
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, true, false, false, false, false },
                { false, false, true, false, true, false, true, false, false },
                { false, false, false, true, false, true, false, false, false },
                { false, true, false, true, true, true, false, true, false },
                { false, false, true, false, true, false, true, false, false },
                { false, false, true, true, false, true, true, false, false },
                { false, false, true, true, true, true, true, false, false },
                { false, false, false, false, false, false, false, false, false }
            };
        }

        public override void IsMoveAvailable(Square[][] matrix)
        {
            if (this.bishop.IsMoveAvailable(this, matrix) ||
                this.rook.IsMoveAvailable(this, matrix))
            {
                this.IsMovable = true;
            }
            else
            {
                this.IsMovable = false;
            }
        }

        public override void Attacking(Square[][] matrix)
        {
            this.rook.Attacking(this, matrix);
            this.bishop.Attacking(this, matrix);
        }

        public override bool Move(Position to, Square[][] matrix)
        {
            if (this.bishop.Move(this, to, matrix) || this.rook.Move(this, to, matrix))
            {
                return true;
            }

            return false;
        }

        public override bool Take(Position to, Square[][] matrix)
        {
            return this.Move(to, matrix);
        }
    }
}
