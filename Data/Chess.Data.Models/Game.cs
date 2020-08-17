namespace Chess.Data.Models
{
    using System;

    using Chess.Common;
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
                GlobalConstants.TurnCounter++;

                if (GlobalConstants.GameOver.ToString() != GameOver.None.ToString())
                {
                    this.OnGameOver?.Invoke(this.MovingPlayer, new GameOverEventArgs(GlobalConstants.GameOver));
                }

                this.ChangeTurns();

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
    }
}
