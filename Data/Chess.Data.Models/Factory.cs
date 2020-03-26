namespace Chess.Data.Models
{
    using Chess.Common;
    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces;

    public class Factory
    {
        public static Piece GetPawn(Color color)
        {
            Piece pawn = new Pawn(color);
            return pawn;
        }

        public static Piece GetRook(Color color)
        {
            Piece rook = new Rook(color);
            return rook;
        }

        public static Piece GetKnight(Color color)
        {
            Piece knight = new Knight(color);
            return knight;
        }

        public static Piece GetBishop(Color color)
        {
            Piece bishop = new Bishop(color);
            return bishop;
        }

        public static Piece GetQueen(Color color)
        {
            Piece queen = new Queen(color);
            return queen;
        }

        public static Piece GetKing(Color color)
        {
            Piece king = new King(color);
            return king;
        }

        public static Piece GetNone()
        {
            Piece none = new None();
            return none;
        }

        public static Square GetSquare(PositionX col, PositionY row)
        {
            Square square = new Square(col, row);
            return square;
        }

        public static Square[][] GetBoardMatrix()
        {
            Square[][] matrix = new Square[GlobalConstants.BoardRows][];

            for (int row = 0; row < GlobalConstants.BoardRows; row++)
            {
                matrix[row] = new Square[GlobalConstants.BoardCols];
            }

            return matrix;
        }
    }
}
