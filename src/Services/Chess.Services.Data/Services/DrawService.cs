namespace Chess.Services.Data.Services
{
    using System.Collections.Generic;

    using Chess.Common.Constants;
    using Chess.Common.Enums;
    using Chess.Services.Data.Models;
    using Chess.Services.Data.Services.Contracts;

    public class DrawService : IDrawService
    {
        private readonly Queue<string> threefoldQueue = new ();
        private readonly Queue<string> fivefoldQueue = new ();
        private int fiftyMoveCounter;

        public bool IsStalemate(Board board, Player opponent)
        {
            for (int rank = BoardConstants.Rank8; rank <= BoardConstants.Rank1; rank++)
            {
                for (int file = BoardConstants.FileA; file <= BoardConstants.FileH; file++)
                {
                    var currentFigure = board
                        .GetSquareByCoordinates(rank, file).Piece;

                    if (currentFigure != null &&
                        currentFigure.Color == opponent.Color)
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
            int counterWhite = 0;
            int counterBlack = 0;

            for (int rank = BoardConstants.Rank8; rank <= BoardConstants.Rank1; rank++)
            {
                for (int file = BoardConstants.FileA; file <= BoardConstants.FileH; file++)
                {
                    var currentFigure = board
                        .GetSquareByCoordinates(rank, file).Piece;

                    if (currentFigure != null &&
                       !currentFigure.IsType(SymbolConstants.King))
                    {
                        if (currentFigure.IsType(SymbolConstants.Pawn, SymbolConstants.Rook, SymbolConstants.Queen) ||
                            counterWhite > 1 || counterBlack > 1)
                        {
                            return false;
                        }

                        if (currentFigure.Color == Color.White)
                        {
                            counterWhite++;
                        }
                        else
                        {
                            counterBlack++;
                        }
                    }
                }
            }

            return true;
        }

        public bool IsThreefoldRepetionDraw(string fen)
        {
            this.threefoldQueue.Enqueue(fen);

            if (this.threefoldQueue.Count == 9)
            {
                if (this.IsThreefoldDraw(fen, this.threefoldQueue.ToArray()))
                {
                    this.threefoldQueue.Dequeue();
                    return true;
                }

                this.threefoldQueue.Dequeue();
            }

            return false;
        }

        public bool IsFivefoldRepetitionDraw(string fen)
        {
            this.fivefoldQueue.Enqueue(fen);

            if (this.fivefoldQueue.Count == 17)
            {
                if (this.IsFivefoldDraw(fen, this.fivefoldQueue.ToArray()))
                {
                    return true;
                }

                this.fivefoldQueue.Dequeue();
            }

            return false;
        }

        public bool IsFiftyMoveDraw(Move move)
        {
            if (!move.Target.Piece.IsType(SymbolConstants.Pawn) && move.Type != MoveType.Taking)
            {
                this.fiftyMoveCounter += 1;

                if (this.fiftyMoveCounter == 100)
                {
                    return true;
                }

                return false;
            }

            this.fiftyMoveCounter = 0;
            return false;
        }

        private bool IsThreefoldDraw(string currentFen, string[] allFens)
        {
            if (string.Compare(currentFen, allFens[CommonConstants.ArrayPositionOne]) == 0 &&
                string.Compare(currentFen, allFens[CommonConstants.ArrayPositionFive]) == 0)
            {
                return true;
            }

            return false;
        }

        private bool IsFivefoldDraw(string currentFen, string[] allFens)
        {
            if (this.IsThreefoldDraw(currentFen, allFens) &&
                string.Compare(currentFen, allFens[CommonConstants.ArrayPositionNine]) == 0 &&
                string.Compare(currentFen, allFens[CommonConstants.ArrayPositionThirteen]) == 0)
            {
                return true;
            }

            return false;
        }
    }
}
