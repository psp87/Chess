namespace Chess.Models
{
    using System;

    using Enums;
    using EventArgs;
    using View;

    public class Game
    {
        private Print printer;
        private Draw drawer;

        public Game()
        {
            this.printer = Factory.GetPrint();
            this.drawer = Factory.GetDraw();

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
            this.printer.PlayersMenu(Color.Light);
            var namePlayer1 = Console.ReadLine();
            this.printer.PlayersMenu(Color.Dark);
            var namePlayer2 = Console.ReadLine();

            Player player1 = Factory.GetPlayer(namePlayer1.ToUpper(), Color.Light);
            Player player2 = Factory.GetPlayer(namePlayer2.ToUpper(), Color.Dark);

            this.Player1 = player1;
            this.Player2 = player2;
        }

        public void New()
        {
            this.ChessBoard.Initialize();

            this.printer.Stats(this.MovingPlayer, this.Opponent);
            this.printer.ExampleText();
            this.printer.GameMenu();
            this.drawer.BoardOrientate(this.ChessBoard.Matrix, this.MovingPlayer.Color);
        }

        public void End()
        {
            Console.ReadLine();
            Console.Clear();
            this.drawer.BoardEmpty(Color.Light);
        }

        public void Start()
        {
            while (Globals.GameOver.ToString() == GameOver.None.ToString())
            {
                Globals.TurnCounter++;

                this.printer.Turn(this.MovingPlayer);
                this.ChessBoard.MakeMove(this.MovingPlayer, this.Opponent);

                if (Globals.GameOver.ToString() != GameOver.None.ToString())
                {
                    this.OnGameOver?.Invoke(this.MovingPlayer, new GameOverEventArgs(Globals.GameOver));
                }

                this.printer.Stats(this.Player1, this.Player2);
                this.ChangeTurns();

                this.drawer.BoardOrientate(this.ChessBoard.Matrix, this.MovingPlayer.Color);
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
