namespace Chess.Web.Hubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Chess.Common.Enums;
    using Chess.Data;
    using Chess.Data.Models;

    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;

    public partial class GameHub : Hub
    {
        private readonly ConcurrentDictionary<string, Player> players;
        private readonly ConcurrentDictionary<string, Game> games;
        private readonly List<Player> waitingPlayers;
        private readonly IServiceProvider sp;

        public GameHub(IServiceProvider sp)
        {
            this.players = new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);
            this.games = new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);
            this.waitingPlayers = new List<Player>();
            this.sp = sp;
        }

        public override async Task OnConnectedAsync()
        {
            await this.LobbySendInternalMessage(this.Context.User.Identity.Name);
            await this.Clients.Caller.SendAsync("ListRooms", this.waitingPlayers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (this.players.Keys.Contains(this.Context.ConnectionId))
            {
                var leavingPlayer = this.players[this.Context.ConnectionId];

                if (leavingPlayer.GameId != null)
                {
                    var game = this.games[leavingPlayer.GameId];

                    if (game.GameOver == GameOver.None)
                    {
                        await this.GameSendInternalMessage(game.Id, leavingPlayer.Name, null);
                        await this.Clients.Group(leavingPlayer.GameId).SendAsync("GameOver", leavingPlayer, GameOver.Disconnected);

                        if (game.Turn > 30)
                        {
                            var winner = game.MovingPlayer.Id != leavingPlayer.Id ? game.MovingPlayer : game.Opponent;
                            if (this.players.Keys.Contains(winner.Id))
                            {
                                this.UpdateStats(winner, leavingPlayer, game, GameOver.Disconnected);
                            }
                        }
                    }
                }
                else
                {
                    this.waitingPlayers.Remove(leavingPlayer);
                    await this.Clients.All.SendAsync("ListRooms", this.waitingPlayers);
                }

                this.players.TryRemove(leavingPlayer.Id, out _);
            }
        }

        private Player GetPlayer()
        {
            return this.players[this.Context.ConnectionId];
        }

        private Player GetOpponentPlayer(Game game, Player player)
        {
            return game.MovingPlayer.Id != player.Id ? game.MovingPlayer : game.Opponent;
        }

        private Game GetGame(Player player)
        {
            return this.games[player.GameId];
        }

        private int GetUserRating(Player player)
        {
            using var scope = this.sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return dbContext.Stats.Where(x => x.Owner.Id == player.UserId).Select(x => x.Rating).FirstOrDefault();
        }

        private void UpdateStats(Player sender, Player opponent, Game game, GameOver gameOver)
        {
            using var scope = this.sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var senderStats = dbContext.Stats.Where(x => x.Owner.Id == sender.UserId).FirstOrDefault();
            var opponentStats = dbContext.Stats.Where(x => x.Owner.Id == opponent.UserId).FirstOrDefault();

            if (senderStats == null)
            {
                senderStats = new Stats
                {
                    Games = 0,
                    Wins = 0,
                    Draws = 0,
                    Losses = 0,
                    Rating = 1200,
                    OwnerId = sender.UserId,
                };

                dbContext.Stats.Add(senderStats);
                dbContext.SaveChanges();
            }

            if (opponentStats == null)
            {
                opponentStats = new Stats
                {
                    Games = 0,
                    Wins = 0,
                    Draws = 0,
                    Losses = 0,
                    Rating = 1200,
                    OwnerId = opponent.UserId,
                };

                dbContext.Stats.Add(opponentStats);
                dbContext.SaveChanges();
            }

            senderStats.Games += 1;
            opponentStats.Games += 1;

            if (gameOver == GameOver.Checkmate || gameOver == GameOver.Resign || gameOver == GameOver.Disconnected)
            {
                int points = game.CalculateRatingPoints(senderStats.Rating, opponentStats.Rating);

                senderStats.Wins += 1;
                senderStats.Rating += points;

                opponentStats.Losses += 1;
                opponentStats.Rating -= points;
            }
            else if (gameOver == GameOver.Stalemate || gameOver == GameOver.Draw || gameOver == GameOver.ThreefoldDraw || gameOver == GameOver.FivefoldDraw)
            {
                senderStats.Draws += 1;
                opponentStats.Draws += 1;
            }

            dbContext.Stats.Update(senderStats);
            dbContext.Stats.Update(opponentStats);
            dbContext.SaveChanges();
        }
    }
}
