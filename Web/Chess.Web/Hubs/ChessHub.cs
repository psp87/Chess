namespace Chess.Web.Hubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    using Chess.Common.Enums;
    using Chess.Data.Models;
    using Chess.Data.Models.EventArgs;

    using Microsoft.AspNetCore.SignalR;

    public class ChessHub : Hub
    {
        #region Private Variables
        private readonly ConcurrentDictionary<string, Player> players =
            new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Game> games =
            new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentQueue<Player> waitingPlayers =
            new ConcurrentQueue<Player>();
        #endregion

        public async Task FindGame(string username)
        {
            Player joiningPlayer = Factory.GetPlayer(username, this.Context.ConnectionId);
            this.players[joiningPlayer.Id] = joiningPlayer;
            await this.Clients.Caller.SendAsync("PlayerJoined", joiningPlayer);

            this.waitingPlayers.TryDequeue(out Player opponent);

            if (opponent == null)
            {
                this.waitingPlayers.Enqueue(joiningPlayer);
                await this.Clients.Caller.SendAsync("WaitingList");
            }
            else
            {
                joiningPlayer.Color = Color.Dark;
                opponent.Color = Color.Light;

                Game game = Factory.GetGame(opponent, joiningPlayer);
                this.games[game.Id] = game;

                game.OnGameOver += this.Game_OnGameOver;

                await Task.WhenAll(
                    this.Groups.AddToGroupAsync(game.Player1.Id, groupName: game.Id),
                    this.Groups.AddToGroupAsync(game.Player2.Id, groupName: game.Id),
                    this.Clients.Group(game.Id).SendAsync("Start", game));
            }
        }

        public async Task MoveSelected(string source, string target)
        {
            var player = this.players[this.Context.ConnectionId];
            //if (!player.HasToMove)
            //{
            //    return;
            //}

            var game = this.GetGame(player, out Player opponent);

            //var move = game.MoveSelected(source, target, player, opponent);
            await this.Clients.All.SendAsync("MoveDone", source, target, game);
        }

        private Game GetGame(Player player, out Player opponent)
        {
            opponent = null;
            Game foundGame = this.games.Values.FirstOrDefault(g => g.Id == player.GameId);

            if (foundGame == null)
            {
                return null;
            }

            opponent = (player.Id == foundGame.Player1.Id) ? foundGame.Player2 : foundGame.Player1;
            return foundGame;
        }

        private void Game_OnGameOver(object sender, EventArgs e)
        {
            var player = sender as Player;
            var gameOver = e as GameOverEventArgs;

            this.Clients.All.SendAsync("GameOver", gameOver.GameOver, player);
        }
    }
}
