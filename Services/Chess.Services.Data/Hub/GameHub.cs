namespace Chess.Services.Data.Hub
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    using Chess.Common.Enums;
    using Chess.Data.Models;
    using Chess.Data.Models.EventArgs;
    using Microsoft.AspNetCore.SignalR;

    public class GameHub : Hub
    {
        private readonly ConcurrentDictionary<string, Player> players =
            new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Game> games =
            new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentQueue<Player> waitingPlayers =
            new ConcurrentQueue<Player>();

        public async Task Start(string username)
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
                game.OnGameOver += this.Game_OnGameOver;
                this.games[game.Id] = game;

                await Task.WhenAll(
                    this.Groups.AddToGroupAsync(game.Player1.Id, groupName: game.Id),
                    this.Groups.AddToGroupAsync(game.Player2.Id, groupName: game.Id),
                    this.Clients.Group(game.Id).SendAsync("Start", game));

                game.Start();
            }
        }

        public async Task MoveSelected(string a, string b)
        {
            var player = this.players[Context.ConnectionId];
            if (!player.HasToMove)
                return;

            var game = GetGame(player, out Player opponent);
            var start = game.ChessBoard.GetSquareAtPosition(a);
            var end = game.ChessBoard.GetSquareAtPosition(b);
            var move = game.MoveSelected(start, end);
            await Clients.All.SendAsync("MoveDone", game, move);
        }

        public async Task PieceSelected(int x, int y)
        {
            var player = this.players[Context.ConnectionId];
            if (!player.HasToMove)
                return;

            var game = GetGame(player, out Player opponent);
            var square = game.ChessBoard.GetSquareAtPosition(new Position(x, y));
            game.ChessBoard.ClearBoardSelections();
            game.ChessBoard.CalculatePossibleMovesForPiece(square.Piece);
            square.IsSelected = true;
            await Clients.All.SendAsync("ShowPossibleMoves", game);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Player leavingPlayer;
            this.players.TryGetValue(Context.ConnectionId, out leavingPlayer);

            if (leavingPlayer != null)
            {
                Player opponent;
                Game ongoingGame = GetGame(leavingPlayer, out opponent);
                if (ongoingGame != null)
                {
                    this.Clients.Group(ongoingGame.Id).SendAsync("OpponentLeft", leavingPlayer, opponent);
                    RemoveGame(ongoingGame.Id);
                }
            }

            return base.OnDisconnectedAsync(exception);
        }

        #region private methods
        private Game GetGame(Player player, out Player opponent)
        {
            opponent = null;
            Game foundGame = games.Values.FirstOrDefault(g => g.Id == player.GameId);

            if (foundGame == null)
                return null;

            opponent = (player.Id == foundGame.Player1.Id) ? foundGame.Player2 : foundGame.Player1;
            return foundGame;
        }

        private void RemoveGame(string gameId)
        {
            Game foundGame;
            if (!games.TryRemove(gameId, out foundGame))
                return;

            this.players.TryRemove(foundGame.Player1.Id, out Player foundPlayer1);
            this.players.TryRemove(foundGame.Player2.Id, out Player foundPlayer2);
        }
        #endregion

        private void Game_OnGameOver(object sender, EventArgs e)
        {
            var player = sender as Player;
            var gameOver = e as GameOverEventArgs;

            this.Clients.All.SendAsync("GameOver", gameOver.GameOver, player);
        }
    }
}
