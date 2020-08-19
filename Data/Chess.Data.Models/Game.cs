namespace Chess.Data.Models
{
    using System;
    using System.Collections.Generic;

    using Chess.Common;
    using Chess.Common.Enums;
    using Chess.Data.Models.EventArgs;
    using Chess.Data.Models.Pieces;

    public class Game
    {
        private Queue<string> movesThreefold;
        private Queue<string> movesFivefold;

        private string[] arrayThreefold;
        private string[] arrayFivefold;

        public Game(Player player1, Player player2)
        {
            this.movesThreefold = new Queue<string>();
            this.movesFivefold = new Queue<string>();

            this.arrayThreefold = new string[9];
            this.arrayFivefold = new string[17];

            this.ChessBoard = Factory.GetBoard();
            this.ChessBoard.Initialize();

            this.Player1 = player1;
            this.Player2 = player2;

            this.Id = Guid.NewGuid().ToString();
            this.Player1.GameId = this.Id;
            this.Player2.GameId = this.Id;
        }

        public event EventHandler OnGameOver;

        public string Id { get; set; }

        public Board ChessBoard { get; set; }

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public Player MovingPlayer => this.Player1?.HasToMove ?? false ? this.Player1 : this.Player2;

        public Player Opponent => this.Player1?.HasToMove ?? false ? this.Player2 : this.Player1;

        public bool MoveSelected(string source, string target, string sourceFen, string targetFen)
        {
            if (this.ChessBoard.MakeMove(source, target, sourceFen, targetFen, this.MovingPlayer, this.Opponent))
            {
                this.IsThreefoldRepetionDraw(targetFen);
                this.IsFivefoldRepetitionDraw(targetFen);
                this.IsDraw();
                this.IsStalemate(this.Opponent);

                if (GlobalConstants.GameOver.ToString() != GameOver.None.ToString())
                {
                    this.OnGameOver?.Invoke(this.MovingPlayer, new GameOverEventArgs(GlobalConstants.GameOver));
                }

                this.ChangeTurns();
                GlobalConstants.TurnCounter++;

                return true;
            }

            return false;
        }

        private void ChangeTurns()
        {
            if (this.Player1.HasToMove)
            {
                this.Player1.HasToMove = false;
                this.Player2.HasToMove = true;
            }
            else
            {
                this.Player2.HasToMove = false;
                this.Player1.HasToMove = true;
            }
        }

        private void IsStalemate(Player player)
        {
            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                for (int x = 0; x < GlobalConstants.BoardCols; x++)
                {
                    var currentFigure = this.ChessBoard.Matrix[y][x].Piece;

                    if (currentFigure.Color == player.Color)
                    {
                        currentFigure.IsMoveAvailable(this.ChessBoard.Matrix);
                        if (currentFigure.IsMovable)
                        {
                            return;
                        }
                    }
                }
            }

            GlobalConstants.GameOver = GameOver.Stalemate;
        }

        private void IsThreefoldRepetionDraw(string fen)
        {
            this.movesThreefold.Enqueue(fen);

            GlobalConstants.IsThreefoldDraw = false;

            if (this.movesThreefold.Count == 9)
            {
                this.arrayThreefold = this.movesThreefold.ToArray();

                var isFirstFenSame = string.Compare(fen, this.arrayThreefold[0]) == 0;
                var isFiveFenSame = string.Compare(fen, this.arrayThreefold[4]) == 0;

                if (isFirstFenSame && isFiveFenSame)
                {
                    GlobalConstants.IsThreefoldDraw = true;
                }

                this.movesThreefold.Dequeue();
            }
        }

        private void IsFivefoldRepetitionDraw(string fen)
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
                    GlobalConstants.GameOver = GameOver.FivefoldDraw;
                }
                else
                {
                    this.movesFivefold.Dequeue();
                }
            }
        }

        private void IsDraw()
        {
            int counterBishopKnightWhite = 0;
            int counterBishopKnightBlack = 0;

            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                for (int x = 0; x < GlobalConstants.BoardCols; x++)
                {
                    var currentFigure = this.ChessBoard.Matrix[y][x].Piece;

                    if (!(currentFigure is Empty || currentFigure is King))
                    {
                        if (currentFigure is Pawn ||
                            currentFigure is Rook ||
                            currentFigure is Queen ||
                            counterBishopKnightWhite > 1 ||
                            counterBishopKnightBlack > 1)
                        {
                            return;
                        }

                        if (currentFigure.Color == Color.Light)
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

            GlobalConstants.GameOver = GameOver.Draw;
        }
    }
}
