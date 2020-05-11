namespace Chess.Models.Pieces
{
    using Enums;
    using Helpers;

    public class Bishop : Piece
    {
        private BishopBahaviour bishop;

        public Bishop(Color color)
            : base(color)
        {
            this.bishop = Factory.GetBishopBehaviour();
        }

        public override char Symbol => 'B';

        public override bool[,] FigureMatrix 
        { 
            get => new bool[Globals.CellRows, Globals.CellCols]
            {
                { false, false, false, false, false, false, false, false, false },
                { false, false, false, false, true, false, false, false, false },
                { false, false, false, true, true, true, false, false, false },
                { false, false, true, true, false, true, true, false, false },
                { false, false, true, false, false, false, true, false, false },
                { false, false, false, true, false, true, false, false, false },
                { false, false, false, false, true, false, false, false, false },
                { false, false, true, true, false, true, true, false, false },
                { false, false, false, false, false, false, false, false, false }
            };
        }

        public override void IsMoveAvailable(Square[][] matrix)
        {
            this.IsMovable = this.bishop.IsMoveAvailable(this, matrix) ? true : false;
        }

        public override void Attacking(Square[][] matrix)
        {
            this.bishop.Attacking(this, matrix);
        }

        public override bool Move(Position to, Square[][] matrix)
        {
            return this.bishop.Move(this, to, matrix);
        }

        public override bool Take(Position to, Square[][] matrix)
        {
            return this.Move(to, matrix);
        }
    }
}
