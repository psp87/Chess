namespace Chess.Web.Hubs
{
    using System;
    using System.Threading.Tasks;

    using Chess.Common.Enums;
    using Chess.Data.Models;
    using Chess.Data.Models.EventArgs;

    using Microsoft.AspNetCore.SignalR;

    public partial class GameHub
    {
        public async Task MoveSelected(string source, string target, string sourceFen, string targetFen)
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];

            if (!player.HasToMove ||
                !game.MakeMove(source, target, targetFen))
            {
                await this.Clients.Caller.SendAsync("BoardSnapback", sourceFen);
                return;
            }

            await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("BoardMove", source, target);
            await this.Clients.Group(game.Id).SendAsync("HighlightMove", source, target, game.Opponent);

            if (game.GameOver.ToString() == GameOver.None.ToString())
            {
                await this.Clients.Group(game.Id).SendAsync("UpdateStatus", game.MovingPlayer.Name);
            }

            if (game.Move.Type != MoveType.Normal)
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

                game.Move.Type = MoveType.Normal;
            }
        }

        public async Task IsThreefoldDraw()
        {
            var movingPlayer = this.players[this.Context.ConnectionId];
            var game = this.games[movingPlayer.GameId];
            var opponent = game.Opponent;

            if (movingPlayer.IsThreefoldDrawAvailable && movingPlayer.HasToMove)
            {
                game.GameOver = GameOver.ThreefoldDraw;
                await this.Clients.Group(game.Id).SendAsync("GameOver", movingPlayer, GameOver.ThreefoldDraw);

                await this.GameSendInternalMessage(game.Id, movingPlayer.Name, null);
                this.UpdateStats(movingPlayer, opponent, game, GameOver.ThreefoldDraw);
            }
        }

        public async Task Resign()
        {
            var loser = this.players[this.Context.ConnectionId];
            var game = this.games[loser.GameId];
            var winner = game.MovingPlayer.Id != loser.Id ? game.MovingPlayer : game.Opponent;

            game.GameOver = GameOver.Resign;
            await this.Clients.Group(game.Id).SendAsync("GameOver", loser, GameOver.Resign);

            await this.GameSendInternalMessage(game.Id, loser.Name, null);
            this.UpdateStats(winner, loser, game, GameOver.Resign);
        }

        public async Task OfferDrawRequest()
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];

            await this.GameSendInternalMessage(game.Id, player.Name, null);
            await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("DrawOffered", player);
        }

        public async Task OfferDrawAnswer(bool isAccepted)
        {
            var player = this.players[this.Context.ConnectionId];
            var game = this.games[player.GameId];

            if (isAccepted)
            {
                var opponent = game.MovingPlayer.Id != player.Id ? game.MovingPlayer : game.Opponent;

                game.GameOver = GameOver.Draw;
                await this.Clients.Group(game.Id).SendAsync("GameOver", null, GameOver.Draw);

                await this.GameSendInternalMessage(game.Id, player.Name, null);
                this.UpdateStats(player, opponent, game, GameOver.Draw);
            }
            else
            {
                await this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("DrawOfferRejected", player);
            }
        }

        private void Game_OnGameOver(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var opponent = game.Opponent;
            var gameOver = e as GameOverEventArgs;

            this.Clients.Group(game.Id).SendAsync("GameOver", player, gameOver.GameOver);

            _ = this.GameSendInternalMessage(game.Id, player.Name, gameOver.GameOver.ToString());
            this.UpdateStats(player, opponent, game, gameOver.GameOver);
        }

        private void Game_OnMoveComplete(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var args = e as MoveCompleteEventArgs;

            this.Clients.Group(game.Id).SendAsync("UpdateMoveHistory", player, args.Notation);
        }

        private void Game_OnMoveEvent(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var message = e as MoveEventArgs;

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
            var game = this.games[player.GameId];
            var args = e as TakePieceEventArgs;

            this.Clients.Group(game.Id).SendAsync("UpdateTakenFigures", player, args.PieceName, args.Points);
        }

        private void Game_OnThreefoldDrawAvailable(object sender, EventArgs e)
        {
            var player = sender as Player;
            var game = this.games[player.GameId];
            var args = e as ThreefoldDrawEventArgs;

            this.Clients.Caller.SendAsync("ThreefoldAvailable", false);
            this.Clients.GroupExcept(game.Id, this.Context.ConnectionId).SendAsync("ThreefoldAvailable", args.IsAvailable);
        }
    }
}
