namespace Chess.Common.EventArgs
{
    using System;

    using Chess.Common.Enums;

    public class GameOverEventArgs : EventArgs
    {
        public GameOverEventArgs(GameOver gameOver)
        {
            this.GameOver = gameOver;
        }

        public GameOver GameOver { get; set; }
    }
}
