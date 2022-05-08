namespace Chess.Web.Hubs
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.SignalR;

    public partial class GameHub
    {
        public async Task LobbySendMessage(string message)
        {
            var msgFormat = $"{DateTime.Now:HH:mm}, {this.Context.User.Identity.Name}: {message}";
            await this.Clients.All.SendAsync("UpdateLobbyChat", msgFormat);
        }

        public async Task GameSendMessage(string message)
        {
            var player = this.GetPlayer();
            var game = this.GetGame(player);

            var msgFormat = $"{DateTime.Now:HH:mm}, {player.Name}: {message}";
            await this.Clients.Group(game.Id).SendAsync("UpdateGameChat", msgFormat, player);
        }

        private async Task LobbySendInternalMessage(string name, [CallerMemberName] string caller = "")
        {
            var message = string.Empty;
            switch (caller)
            {
                case nameof(this.OnConnectedAsync):
                    message = $"{name} joined the lobby";
                    break;
                case nameof(this.CreateRoom):
                    message = $"{name} created a room";
                    break;
            }

            await this.Clients.All.SendAsync("UpdateLobbyChatInternalMessage", message);
        }

        private async Task GameSendInternalMessage(string gameId, string name, string gameOver, [CallerMemberName] string caller = "")
        {
            var message = string.Empty;
            switch (caller)
            {
                case nameof(this.Game_OnMoveEvent):
                    message = $"Check announced by {name}!";
                    break;
                case nameof(this.StartGame):
                    message = $"{name} joined. The game started!";
                    break;
                case nameof(this.Game_OnGameOver):
                    message = $"{gameOver}!";
                    break;
                case nameof(this.Resign):
                    message = $"{name} resigned!";
                    break;
                case nameof(this.OfferDrawRequest):
                    message = $"{name} requested a draw!";
                    break;
                case nameof(this.OfferDrawAnswer):
                    if (gameOver == "Draw")
                    {
                        message = $"{name} accepted the offer. Draw!";
                    }
                    else
                    {
                        message = $"{name} rejected the offer!";
                    }

                    break;
                case nameof(this.OnDisconnectedAsync):
                    message = $"{name} left. You win!";
                    break;
                case nameof(this.ThreefoldDraw):
                    message = $"{name} declared threefold draw!";
                    break;
            }

            await this.Clients.Group(gameId).SendAsync("UpdateGameChatInternalMessage", message);
        }
    }
}
