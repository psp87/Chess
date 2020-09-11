namespace Chess.Data.Models
{
    using System;

    using Chess.Common.Enums;
    using Chess.Data.Models.EventArgs;

    public class Game
    {
        public Game(Player player1, Player player2)
        {
            this.ChessBoard = Factory.GetBoard();
            this.ChessBoard.Initialize();

            this.Player1 = player1;
            this.Player2 = player2;

            this.Id = Guid.NewGuid().ToString();
            this.GameOver = GameOver.None;
            this.Player1.GameId = this.Id;
            this.Player2.GameId = this.Id;
            this.Turn = 1;
        }

        public event EventHandler OnNotification;

        public event EventHandler OnGameOver;

        public string Id { get; set; }

        public Board ChessBoard { get; set; }

        public GameOver GameOver { get; set; }

        public int Turn { get; set; }

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public Player MovingPlayer => this.Player1?.HasToMove ?? false ? this.Player1 : this.Player2;

        public Player Opponent => this.Player1?.HasToMove ?? false ? this.Player2 : this.Player1;

        public bool MakeMove(string source, string target, string targetFen)
        {
            if (this.ChessBoard.TryMove(source, target, targetFen, this.MovingPlayer, this.Opponent, this.Turn))
            {
                if (this.ChessBoard.IsPlayerChecked(this.Opponent))
                {
                    this.OnNotification?.Invoke(this.Opponent, new MessageEventArgs(Notification.CheckOpponent));
                    if (this.ChessBoard.IsCheckmate(this.MovingPlayer, this.Opponent))
                    {
                        this.GameOver = GameOver.Checkmate;
                    }
                }

                if (!this.MovingPlayer.IsCheck && !this.Opponent.IsCheck)
                {
                    this.OnNotification?.Invoke(this.MovingPlayer, new MessageEventArgs(Notification.CheckClear));
                }

                this.ChessBoard.IsThreefoldRepetionDraw(targetFen, this.MovingPlayer, this.Opponent);

                if (this.ChessBoard.IsFivefoldRepetitionDraw(targetFen))
                {
                    this.GameOver = GameOver.FivefoldDraw;
                }

                if (this.ChessBoard.IsDraw())
                {
                    this.GameOver = GameOver.Draw;
                }

                if (this.ChessBoard.IsStalemate(this.Opponent))
                {
                    this.GameOver = GameOver.Stalemate;
                }

                if (this.GameOver.ToString() != GameOver.None.ToString())
                {
                    this.OnGameOver?.Invoke(this.MovingPlayer, new GameOverEventArgs(this.GameOver));
                }

                this.Turn++;
                this.ChangeTurns();

                return true;
            }

            if (!this.MovingPlayer.IsCheck)
            {
                this.OnNotification?.Invoke(this.MovingPlayer, new MessageEventArgs(Notification.InvalidMove));
            }

            this.MovingPlayer.IsCheck = false;
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
    }
}
