namespace Chess.Services.Data
{
    using System.Collections.Generic;

    using Chess.Common;
    using Chess.Common.Enums;
    using Chess.Services.Data.Contracts;
    using Chess.Services.Data.Models;
    using Chess.Services.Data.Models.Pieces;

    public class DrawService : IDrawService
    {
        private readonly Queue<string> movesThreefold;
        private readonly Queue<string> movesFivefold;
        private string[] arrayThreefold;
        private string[] arrayFivefold;

        public DrawService()
        {
            this.movesThreefold = new Queue<string>();
            this.movesFivefold = new Queue<string>();
            this.arrayThreefold = new string[9];
            this.arrayFivefold = new string[17];
            this.FiftyMoveCounter = 0;
        }

        public int FiftyMoveCounter { get; set; }

        public bool IsStalemate(Board board, Player opponent)
        {
            for (int rank = 0; rank < Constants.Ranks; rank++)
            {
                for (int file = 0; file < Constants.Files; file++)
                {
                    var currentFigure = board.GetSquareByCoordinates(rank, file).Piece;

                    if (currentFigure != null && currentFigure.Color == opponent.Color)
                    {
                        currentFigure.IsMoveAvailable(board.Matrix);
                        if (currentFigure.IsMovable)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool IsDraw(Board board)
        {
            int counterBishopKnightWhite = 0;
            int counterBishopKnightBlack = 0;

            for (int rank = 0; rank < Constants.Ranks; rank++)
            {
                for (int file = 0; file < Constants.Files; file++)
                {
                    var currentFigure = board.GetSquareByCoordinates(rank, file).Piece;

                    if (!(currentFigure == null || currentFigure is King))
                    {
                        if (currentFigure is Pawn ||
                            currentFigure is Rook ||
                            currentFigure is Queen ||
                            counterBishopKnightWhite > 1 ||
                            counterBishopKnightBlack > 1)
                        {
                            return false;
                        }

                        if (currentFigure.Color == Color.White)
                        {
                            counterBishopKnightWhite++;
                        }
                        else
                        {
                            counterBishopKnightBlack++;
                        }
                    }
                }
            }

            return true;
        }

        public bool IsThreefoldRepetionDraw(string fen)
        {
            this.movesThreefold.Enqueue(fen);

            if (this.movesThreefold.Count == 9)
            {
                this.arrayThreefold = this.movesThreefold.ToArray();

                var isFirstFenSame = string.Compare(fen, this.arrayThreefold[0]) == 0;
                var isFiveFenSame = string.Compare(fen, this.arrayThreefold[4]) == 0;

                this.movesThreefold.Dequeue();

                if (isFirstFenSame && isFiveFenSame)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsFivefoldRepetitionDraw(string fen)
        {
            this.movesFivefold.Enqueue(fen);

            if (this.movesFivefold.Count == 17)
            {
                this.arrayFivefold = this.movesFivefold.ToArray();

                var isFirstFenSame = string.Compare(fen, this.arrayFivefold[0]) == 0;
                var isFiveFenSame = string.Compare(fen, this.arrayFivefold[4]) == 0;
                var isNineFenSame = string.Compare(fen, this.arrayFivefold[8]) == 0;
                var isThirteenFenSame = string.Compare(fen, this.arrayFivefold[12]) == 0;

                if (isFirstFenSame && isFiveFenSame && isNineFenSame && isThirteenFenSame)
                {
                    return true;
                }
                else
                {
                    this.movesFivefold.Dequeue();
                }
            }

            return false;
        }

        public bool IsFiftyMoveDraw(Move move)
        {
            if (!(move.Target.Piece is Pawn) && move.Type != MoveType.Taking)
            {
                this.FiftyMoveCounter += 1;

                if (this.FiftyMoveCounter == 100)
                {
                    return true;
                }

                return false;
            }

            this.FiftyMoveCounter = 0;
            return false;
        }
    }
}
