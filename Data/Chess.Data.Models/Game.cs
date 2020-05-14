namespace Chess.Data.Models
{
    using System;

    using Chess.Common;
    using Chess.Common.Enums;
    using Chess.Data.Models.EventArgs;

    public class Game
    {
        public Game()
        {
            this.ChessBoard = Factory.GetBoard();
        }

        public event EventHandler OnGameOver;

        public Board ChessBoard { get; set; }

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public Player MovingPlayer => this.Player1?.HasToMove ?? false ? this.Player2 : this.Player1;

        public Player Opponent => this.Player1?.HasToMove ?? false ? this.Player1 : this.Player2;

        public void GetPlayers()
        {
            Player player1 = Factory.GetPlayer("Player1".ToUpper(), Color.Light);
            Player player2 = Factory.GetPlayer("Player2".ToUpper(), Color.Dark);

            this.Player1 = player1;
            this.Player2 = player2;
        }

        public void New()
        {
            this.ChessBoard.Initialize();
        }

        public void Start()
        {
            while (GlobalConstants.GameOver.ToString() == GameOver.None.ToString())
            {
                GlobalConstants.TurnCounter++;

                this.ChessBoard.MakeMove(this.MovingPlayer, this.Opponent);

                if (GlobalConstants.GameOver.ToString() != GameOver.None.ToString())
                {
                    this.OnGameOver?.Invoke(this.MovingPlayer, new GameOverEventArgs(GlobalConstants.GameOver));
                }

                this.ChangeTurns();
            }
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
