namespace Chess.Web.Hubs
{
    using System;
    using System.Threading.Tasks;

    using Chess.Common.Enums;
    using Chess.Data.Common.Repositories;
    using Chess.Data.Models;
    using Chess.Services.Data.Models;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;

    public partial class GameHub
    {
        public async Task<Player> CreateRoom(string name)
        {
            Player player = Factory.GetPlayer(name, this.Context.ConnectionId, this.Context.UserIdentifier);
            player.Rating = this.GetUserRating(player);
            this.players[player.Id] = player;

            await this.LobbySendInternalMessage(player.Name);
            await this.Clients.All.SendAsync("AddRoom", player);

            this.waitingPlayers.Add(player);
            return player;
        }

        public async Task<Player> JoinRoom(string name, string id)
        {
            Player player2 = Factory.GetPlayer(name, this.Context.ConnectionId, this.Context.UserIdentifier);
            player2.Rating = this.GetUserRating(player2);
            this.players[player2.Id] = player2;
            var player1 = this.players[id];

            await this.StartGame(player1, player2);
            return player2;
        }

        private async Task StartGame(Player player1, Player player2)
        {
            player1.Color = Color.White;
            player2.Color = Color.Black;
            player1.HasToMove = true;

            var game = Factory.GetGame(player1, player2, this.serviceProvider);
            this.games[game.Id] = game;

            await Task.WhenAll(
                this.Groups.AddToGroupAsync(game.Player1.Id, groupName: game.Id),
                this.Groups.AddToGroupAsync(game.Player2.Id, groupName: game.Id),
                this.Clients.Group(game.Id).SendAsync("Start", game));

            await this.Clients.All.SendAsync("ListRooms", this.waitingPlayers);
            await this.GameSendInternalMessage(game.Id, player2.Name, null);

            this.waitingPlayers.Remove(player1);

            using var scope = this.serviceProvider.CreateScope();

            var gameRepository = scope.ServiceProvider.GetRequiredService<IRepository<GameEntity>>();
            await gameRepository.AddAsync(new GameEntity
                {
                    Id = game.Id,
                    PlayerOneName = game.Player1.Name,
                    PlayerOneUserId = player1.Id,
                    PlayerTwoName = game.Player2.Name,
                    PlayerTwoUserId = player2.Id,
                });

            await gameRepository.SaveChangesAsync();
        }
    }
}
