namespace Chess.Data.Models
{
    using System;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.EventArgs;

    public class Game
    {
        public Game(Player player1, Player player2)
        {
            this.Id = Guid.NewGuid().ToString("D");

            this.ChessBoard = Factory.GetBoard();
            this.ChessBoard.Initialize();

            this.Player1 = player1;
            this.Player2 = player2;

            this.Player1.GameId = this.Id;
            this.Player2.GameId = this.Id;
        }

        public event EventHandler OnGameOver;

        public string Id { get; set; }

        public Board ChessBoard { get; set; }

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public Player MovingPlayer => this.Player1?.HasToMove ?? false ? this.Player1 : this.Player2;

        public Player WaitingPlayer => this.Player1?.HasToMove ?? false ? this.Player2 : this.Player1;

        public Move MoveSelected(Square start, Square end)
        {
            var move = this.ChessBoard.MakeMove(start, end);
            if (move != null)
            {
                this.ChangeTurn();
                this.GameOverCheck();
            }

            return move;
        }

        private void ChangeTurn()
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

        private void GameOverCheck()
        {
            var gameOverCheck = this.ChessBoard.CheckMateOrStalemate(this.MovingPlayer.Color);
            if (gameOverCheck != GameOver.None)
            {
                this.OnGameOver?.Invoke(this.MovingPlayer, new GameOverEventArgs(gameOverCheck));
            }
        }
    }
}
