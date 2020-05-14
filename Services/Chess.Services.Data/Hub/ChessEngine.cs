namespace Chess.Services.Data
{
    using System;

    using Chess.Data.Models;
    using Chess.Data.Models.EventArgs;

    using Microsoft.AspNetCore.SignalR;

    public class ChessEngine : Hub, IChessEngine
    {
        public void Start()
        {
            Game game = Factory.GetGame();

            game.GetPlayers();
            game.New();
            game.OnGameOver += this.Game_OnGameOver;
            game.Start();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        private void Game_OnGameOver(object sender, EventArgs e)
        {
            var player = sender as Player;
            var gameOver = e as GameOverEventArgs;

            this.Clients.All.SendAsync("GameOver", gameOver.GameOver, player);
        }
    }
}
