namespace Chess.Web.Hubs
{
    using System;
    using System.Threading.Tasks;

    using Chess.Common.Enums;
    using Chess.Common.EventArgs;
    using Chess.Data.Common.Repositories;
    using Chess.Data.Models;
    using Chess.Services.Data.Models;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;

    public partial class GameHub
    {
        public async Task MoveSelected(string source, string target, string sourceFen, string targetFen)
        {
            var player = this.GetPlayer();
            var game = this.GetGame(player);

            try
            {
                if (player.HasToMove && game.MakeMove(source, target, targetFen))
                {
                    await this.OpponentBoardMove(source, target, game);
                    await this.HighlightMove(source, target, game);
                    await this.IsSpecialMove(target, game);
                    await this.UpdateStatus(game);
                }
                else
                {
                    await this.Snapback(sourceFen);
                }
            }
            catch (Exception ex)
            {
                using var scope = this.serviceProvider.CreateScope();
                var errorLogRepository = scope.ServiceProvider.GetRequiredService<IRepository<ErrorLogEntity>>();

                await errorLogRepository.AddAsync(new ErrorLogEntity
                {
                    GameId = game.Id,
                    Source = source,
                    Target = target,
                    FenString = sourceFen,
                    ExceptionMessage = ex.Message,
                    CreatedOn = DateTime.Now,
                });

                await errorLogRepository.SaveChangesAsync();
            }
        }

        public async Task ThreefoldDraw()
        {
            var player = this.GetPlayer();
            var game = this.GetGame(player);
            var opponent = game.Opponent;

            game.GameOver = GameOver.ThreefoldDraw;
            await this.Clients
                .Group(game.Id)
                .SendAsync("GameOver", player, game.GameOver);
            await this.GameSendInternalMessage(game.Id, player.Name, null);

            this.UpdateStats(player, opponent, game, game.GameOver);
        }

        public async Task OfferDrawRequest()
        {
            var player = this.GetPlayer();
            var game = this.GetGame(player);

            await this.GameSendInternalMessage(game.Id, player.Name, null);
            await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("DrawOffered", player);
        }

        public async Task OfferDrawAnswer(bool isAccepted)
        {
            var player = this.GetPlayer();
            var game = this.GetGame(player);

            if (isAccepted)
            {
                var opponent = this.GetOpponentPlayer(game, player);

                game.GameOver = GameOver.Draw;
                await this.Clients.Group(game.Id).SendAsync("GameOver", null, game.GameOver);
                await this.GameSendInternalMessage(game.Id, player.Name, game.GameOver.ToString());

                this.UpdateStats(player, opponent, game, game.GameOver);
            }
            else
            {
                await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("DrawOfferRejected", player);
                await this.GameSendInternalMessage(game.Id, player.Name, game.GameOver.ToString());
            }
        }

        public async Task Resign()
        {
            var player = this.GetPlayer();
            var game = this.GetGame(player);
            var opponent = this.GetOpponentPlayer(game, player);

            game.GameOver = GameOver.Resign;
            await this.Clients.Group(game.Id).SendAsync("GameOver", player, game.GameOver);
            await this.GameSendInternalMessage(game.Id, player.Name, null);

            this.UpdateStats(opponent, player, game, game.GameOver);
        }

        private async Task OpponentBoardMove(string source, string target, Game game)
        {
            await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("BoardMove", source, target);
        }

        private async Task HighlightMove(string source, string target, Game game)
        {
            await this.Clients.Group(game.Id).SendAsync("HighlightMove", source, target, game.Opponent);
        }

        private async Task IsSpecialMove(string target, Game game)
        {
            if (game.Move.Type != MoveType.Normal && game.Move.Type != MoveType.Taking)
            {
                switch (game.Move.Type)
                {
                    case MoveType.Castling:
                        await this.Clients.Group(game.Id).SendAsync("BoardMove", game.Move.CastlingArgs.RookSource, game.Move.CastlingArgs.RookTarget);
                        break;
                    case MoveType.EnPassant:
                        await this.Clients.Group(game.Id).SendAsync("EnPassantTake", game.Move.EnPassantArgs.SquareTakenPiece.Name, target);
                        break;
                    case MoveType.PawnPromotion:
                        await this.Clients.Group(game.Id).SendAsync("BoardSetPosition", game.Move.PawnPromotionArgs.FenString);
                        break;
                }
            }

            game.Move.Type = MoveType.Normal;
        }

        private async Task UpdateStatus(Game game)
        {
            if (game.GameOver.ToString() == GameOver.None.ToString())
            {
                await this.Clients.Group(game.Id).SendAsync("UpdateStatus", game.MovingPlayer.Name);
            }
        }

        private async Task Snapback(string sourceFen)
        {
            await this.Clients.Caller.SendAsync("BoardSnapback", sourceFen);
        }

        private void Game_OnGameOver(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.GetGame(player);
            var opponent = game.Opponent;
            var gameOver = e as GameOverEventArgs;

            this.Clients.Group(game.Id).SendAsync("GameOver", player, gameOver.GameOver);
            _ = this.GameSendInternalMessage(game.Id, player.Name, gameOver.GameOver.ToString());

            this.UpdateStats(player, opponent, game, gameOver.GameOver);
        }

        private void Game_OnCompleteMove(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.GetGame(player);
            var args = e as HistoryUpdateArgs;

            this.Clients.Group(game.Id).SendAsync("UpdateMoveHistory", player, args.Notation);
        }

        private void Game_OnMoveEvent(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.GetGame(player);
            var message = e as MoveArgs;

            if (message.Type == Message.CheckOpponent || message.Type == Message.CheckClear)
            {
                if (message.Type == Message.CheckOpponent)
                {
                    _ = this.GameSendInternalMessage(game.Id, player.Name, null);
                }

                this.Clients.Group(game.Id).SendAsync("CheckStatus", message.Type);
            }
            else
            {
                this.Clients.Caller.SendAsync("InvalidMove", message.Type);
            }
        }

        private void Game_OnTakePiece(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.GetGame(player);
            var args = e as TakePieceEventArgs;

            this.Clients.Group(game.Id).SendAsync("UpdateTakenFigures", player, args.PieceName, args.Points);
        }

        private void Game_OnAvailableThreefoldDraw(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.GetGame(player);
            var args = e as ThreefoldDrawEventArgs;

            this.Clients.Caller.SendAsync("ThreefoldAvailable", false);
            this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("ThreefoldAvailable", args.IsAvailable);
        }
    }
}
