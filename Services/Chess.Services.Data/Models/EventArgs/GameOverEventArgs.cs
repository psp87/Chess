namespace Chess.Services.Data.Models.EventArgs
{
    using System;

    using Common.Enums;

    public class GameOverEventArgs : EventArgs
    {
        public GameOverEventArgs(GameOver gameOver)
        {
            this.GameOver = gameOver;
        }

        public GameOver GameOver { get; set; }
    }
}
