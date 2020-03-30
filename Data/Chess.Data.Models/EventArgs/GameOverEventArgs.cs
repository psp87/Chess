namespace Chess.Data.Models.EventArgs
{
    using System;

    using Chess.Data.Models.Enums;

    public class GameOverEventArgs : EventArgs
    {
        public GameOverEventArgs(GameOver gameOver)
        {
            this.GameOver = gameOver;
        }

        public GameOver GameOver { get; set; }
    }
}
