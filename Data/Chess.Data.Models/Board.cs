namespace Chess.Data.Models
{
    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces;

    public class Board
    {
        private Square[][] board;

        public Board()
        {
            this.board = Factory.GetBoardMatrix();
            this.Initialize();
            this.StartPosition();
        }

        public Square[][] BoardMatrix { get => this.board; }

        private void Initialize()
        {
            for (int row = 0; row <= 7; row++)
            {
                for (int col = 0; col <= 7; col++)
                {
                    var posX = (PositionX)col;
                    var posY = (PositionY)row;

                    Square square = Factory.GetSquare(posX, posY);
                    square.Color = (row + col) % 2 == 0 ? Color.Dark : Color.Light;
                    this.board[row][col] = square;
                }
            }
        }

        private void StartPosition()
        {
            this.board[0][0].Piece = Factory.GetRook(Color.Light);
            this.board[0][1].Piece = Factory.GetKnight(Color.Light);
            this.board[0][2].Piece = Factory.GetBishop(Color.Light);
            this.board[0][3].Piece = Factory.GetQueen(Color.Light);
            this.board[0][4].Piece = Factory.GetKing(Color.Light);
            this.board[0][5].Piece = Factory.GetBishop(Color.Light);
            this.board[0][6].Piece = Factory.GetKnight(Color.Light);
            this.board[0][7].Piece = Factory.GetRook(Color.Light);

            this.board[7][0].Piece = Factory.GetRook(Color.Dark);
            this.board[7][1].Piece = Factory.GetKnight(Color.Dark);
            this.board[7][2].Piece = Factory.GetBishop(Color.Dark);
            this.board[7][3].Piece = Factory.GetQueen(Color.Dark);
            this.board[7][4].Piece = Factory.GetKing(Color.Dark);
            this.board[7][5].Piece = Factory.GetBishop(Color.Dark);
            this.board[7][6].Piece = Factory.GetKnight(Color.Dark);
            this.board[7][7].Piece = Factory.GetRook(Color.Dark);

            for (int i = 0; i <= 7; i++)
            {
                Piece lightPawn = Factory.GetPawn(Color.Light);
                this.board[1][i].Piece = lightPawn;

                Piece darkPawn = Factory.GetPawn(Color.Dark);
                this.board[6][i].Piece = darkPawn;
            }

            for (int i = 0; i <= 7; i++)
            {
                this.board[0][i].IsOccupied = true;
                this.board[1][i].IsOccupied = true;
                this.board[6][i].IsOccupied = true;
                this.board[7][i].IsOccupied = true;
            }
        }
    }
}
