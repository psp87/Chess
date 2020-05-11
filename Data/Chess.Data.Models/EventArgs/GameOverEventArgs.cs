namespace Chess.Models.EventArgs
{
    using System;

    using Enums;

    public class GameOverEventArgs : EventArgs
    {
        public GameOverEventArgs(GameOver gameOver)
        {
            this.GameOver = gameOver;
        }

        public GameOver GameOver { get; set; }
    }
}
