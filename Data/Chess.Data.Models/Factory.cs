namespace Chess.Data.Models
{
    using Chess.Common;
    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces;
    using Chess.Data.Models.Pieces.Contracts;

    public class Factory
    {
        public static IPiece GetPawn(Color color)
        {
            return new Pawn(color);
        }

        public static IPiece GetRook(Color color)
        {
            return new Rook(color);
        }

        public static IPiece GetKnight(Color color)
        {
            return new Knight(color);
        }

        public static IPiece GetBishop(Color color)
        {
            return new Bishop(color);
        }

        public static IPiece GetQueen(Color color)
        {
            return new Queen(color);
        }

        public static IPiece GetKing(Color color)
        {
            return new King(color);
        }

        public static Square GetSquare()
        {
            return new Square();
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

        public static Position GetPosition(X posX, Y posY)
        {
            return new Position(posX, posY);
        }

        public static Board GetBoard()
        {
            return new Board();
        }

        public static Player GetPlayer(string name, Color color)
        {
            return new Player(name, color);
        }
    }
}
