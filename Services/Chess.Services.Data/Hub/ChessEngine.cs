namespace Chess.Services.Data
{
    using System;

    using Chess.Common.Enums;
    using Chess.Data.Models;
    using Chess.Data.Models.EventArgs;

    using Microsoft.AspNetCore.
    using Microsoft.AspNetCore.SignalR;

    using Chess.Data.Models;
    using Chess.Data.Models.Enums;
    using Chess.Data.Models.EventArgs;

    public class ChessEngine : Hub, IChessEngine
    {
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

        public void Start()
        {
            string name1 = "Get Name 1";
            string name2 = "Get Name 2";

            Player player1 = Factory.GetPlayer(name1.ToUpper(), Color.Light);
            Player player2 = Factory.GetPlayer(name2.ToUpper(), Color.Dark);

            Game game = Factory.GetGame(player1, player2);

            while (!checkmate && !stalemate)
            {
            }
        }

        private void Game_OnGameOver(object sender, GameOverEventArgs e)
        {
            Clients.All.SendAsync("GameOver", e.GameOver, sender as Player);
        }
    }
}