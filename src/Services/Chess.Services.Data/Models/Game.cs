namespace Chess.Services.Data.Models
{
    using System;
    using System.Threading.Tasks;

    using Chess.Common.Constants;
    using Chess.Common.Enums;
    using Chess.Data.Common.Repositories;
    using Chess.Data.Models;
    using Chess.Services.Data.Dtos;
    using Chess.Services.Data.Services.Contracts;
    using Microsoft.Extensions.DependencyInjection;

    public class Game
    {
        private readonly INotificationService notificationService;
        private readonly ICheckService checkService;
        private readonly IDrawService drawService;
        private readonly IUtilityService utilityService;
        private readonly IServiceProvider serviceProvider;

        public Game(
            Player player1,
            Player player2,
            INotificationService notificationService,
            ICheckService checkService,
            IDrawService drawService,
            IUtilityService utilityService,
            IServiceProvider serviceProvider)
        {
            this.notificationService = notificationService;
            this.checkService = checkService;
            this.drawService = drawService;
            this.utilityService = utilityService;
            this.serviceProvider = serviceProvider;
            this.Player1 = player1;
            this.Player2 = player2;
            this.Player1.GameId = this.Id;
            this.Player2.GameId = this.Id;

            this.ChessBoard.ArrangePieces();
        }

        public string Id { get; } = Guid.NewGuid().ToString();

        public Board ChessBoard { get; } = Factory.GetBoard();

        public Move Move { get; set; } = Factory.GetMove();

        public GameOver GameOver { get; set; } = GameOver.None;

        public int Turn { get; set; } = 1;

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public Player MovingPlayer => this.Player1?.HasToMove ?? false ? this.Player1 : this.Player2;

        public Player Opponent => this.Player1?.HasToMove ?? false ? this.Player2 : this.Player1;

        public bool MakeMove(string source, string target, string targetFen)
        {
            this.Move.Source = this.ChessBoard
                .GetSquareByName(source);
            this.Move.Target = this.ChessBoard
                .GetSquareByName(target);

            var oldSource = this.Move.Source.Clone() as Square;
            var oldTarget = this.Move.Target.Clone() as Square;
            var oldBoard = this.ChessBoard.Clone() as Board;
            var oldIsCheck = this.MovingPlayer.IsCheck;

            if (this.MovePiece() || this.TakePiece() || this.EnPassantTake())
            {
                this.IsPawnPromotion(targetFen);
                this.notificationService.ClearCheck(this.MovingPlayer, this.Opponent);
                this.checkService.IsCheck(this.Opponent, this.ChessBoard);
                this.UpdateHistory(oldSource, oldTarget, oldBoard).GetAwaiter().GetResult();
                this.IsGameOver(targetFen);
                this.ChangeTurns();
                this.Turn++;

                return true;
            }

            this.notificationService.InvalidMove(oldIsCheck, this.MovingPlayer);

            return false;
        }

        public bool TryMove(Player player, Move move)
        {
            var oldPiece = move.Target.Piece;
            this.ChessBoard.ShiftPiece(move.Source, move.Target);

            if (this.checkService.IsCheck(player, this.ChessBoard))
            {
                this.ChessBoard.ShiftPiece(move.Target, move.Source, oldPiece);
                return false;
            }

            if (player == this.Opponent)
            {
                this.ChessBoard.ShiftPiece(move.Target, move.Source, oldPiece);
            }

            return true;
        }

        private bool MovePiece()
        {
            if (this.Move.Target.Piece == null &&
                this.MovingPlayer.Color == this.Move.Source.Piece.Color &&
                this.Move.Source.Piece.Move(this.Move.Target.Position, this.ChessBoard.Matrix, this.Turn, this.Move))
            {
                if (!this.TryMove(this.MovingPlayer, this.Move))
                {
                    this.MovingPlayer.IsCheck = true;
                    return false;
                }

                this.MovingPlayer.IsCheck = false;
                this.Move.Target.Piece.IsFirstMove = false;
                return true;
            }

            return false;
        }

        private bool TakePiece()
        {
            if (this.Move.Target.Piece != null &&
                this.Move.Target.Piece.Color != this.Move.Source.Piece.Color &&
                this.MovingPlayer.Color == this.Move.Source.Piece.Color &&
                this.Move.Source.Piece.Take(this.Move.Target.Position, this.ChessBoard.Matrix, this.Turn, this.Move))
            {
                var piece = this.Move.Target.Piece;

                if (!this.TryMove(this.MovingPlayer, this.Move))
                {
                    this.MovingPlayer.IsCheck = true;
                    return false;
                }

                this.MovingPlayer.IsCheck = false;
                this.Move.Target.Piece.IsFirstMove = false;
                this.MovingPlayer.TakeFigure(piece.Name);
                this.MovingPlayer.Points += piece.Points;
                this.Move.Type = MoveType.Taking;
                this.notificationService.UpdateTakenPiecesHistory(this.MovingPlayer, piece.Name);

                return true;
            }

            return false;
        }

        private bool EnPassantTake()
        {
            if (this.ValidEnPassant())
            {
                if (!this.TryEnPassant())
                {
                    this.MovingPlayer.IsCheck = true;
                    return false;
                }

                this.MovingPlayer.IsCheck = false;
                this.MovingPlayer.TakeFigure(this.Move.Target.Piece.Name);
                this.MovingPlayer.Points += this.Move.Target.Piece.Points;
                this.Move.EnPassantArgs.SquareAvailable = null;
                this.notificationService.UpdateTakenPiecesHistory(this.MovingPlayer, this.Move.Target.Piece.Name);
                return true;
            }

            return false;
        }

        private bool ValidEnPassant()
        {
            if (this.ValidTargetSquare() &&
                this.Move.Source.Piece.IsType(SymbolConstants.Pawn) &&
                this.ValidSourcePosition())
            {
                return true;
            }

            return false;
        }

        private bool ValidTargetSquare()
        {
            return this.Move.EnPassantArgs.SquareAvailable != null &&
                this.Move.EnPassantArgs.Turn == this.Turn &&
                this.Move.EnPassantArgs.SquareAvailable.Equals(this.Move.Target);
        }

        private bool ValidSourcePosition()
        {
            var offsetPlayer = this.MovingPlayer.Color == Color.White ? 1 : -1;

            var position1 = Factory.GetPosition(
                this.Move.EnPassantArgs.SquareAvailable.Position.Rank + offsetPlayer,
                this.Move.EnPassantArgs.SquareAvailable.Position.File + 1);
            var position2 = Factory.GetPosition(
                this.Move.EnPassantArgs.SquareAvailable.Position.Rank + offsetPlayer,
                this.Move.EnPassantArgs.SquareAvailable.Position.File - 1);

            if (this.Move.Source.Position.Equals(position1) ||
                this.Move.Source.Position.Equals(position2))
            {
                return true;
            }

            return false;
        }

        private bool TryEnPassant()
        {
            var neighbourSquare = this.ChessBoard
               .GetSquareByCoordinates(
                    this.Move.Source.Position.Rank,
                    this.Move.Target.Position.File);

            this.ChessBoard.ShiftEnPassant(this.Move.Source, this.Move.Target, neighbourSquare);

            if (this.checkService.IsCheck(this.MovingPlayer, this.ChessBoard))
            {
                this.ChessBoard.ShiftEnPassant(this.Move.Target, this.Move.Source, neighbourSquare, neighbourSquare.Piece);
                return false;
            }

            this.Move.EnPassantArgs.SquareTakenPiece = neighbourSquare;
            this.Move.Type = MoveType.EnPassant;
            return true;
        }

        private void IsGameOver(string targetFen)
        {
            if (this.checkService.IsCheck(this.Opponent, this.ChessBoard))
            {
                this.notificationService.SendCheck(this.MovingPlayer);

                if (this.checkService.IsCheckmate(this.ChessBoard, this.MovingPlayer, this.Opponent, this))
                {
                    this.GameOver = GameOver.Checkmate;
                }
            }

            this.MovingPlayer.IsThreefoldDrawAvailable = false;
            this.notificationService.SendThreefoldDrawAvailability(this.MovingPlayer, false);

            if (this.drawService.IsThreefoldRepetionDraw(targetFen))
            {
                this.Opponent.IsThreefoldDrawAvailable = true;
                this.notificationService.SendThreefoldDrawAvailability(this.MovingPlayer, true);
            }

            if (this.drawService.IsFivefoldRepetitionDraw(targetFen))
            {
                this.GameOver = GameOver.FivefoldDraw;
            }

            if (this.drawService.IsFiftyMoveDraw(this.Move))
            {
                this.GameOver = GameOver.FiftyMoveDraw;
            }

            if (this.drawService.IsDraw(this.ChessBoard))
            {
                this.GameOver = GameOver.Draw;
            }

            if (this.drawService.IsStalemate(this.ChessBoard, this.Opponent))
            {
                this.GameOver = GameOver.Stalemate;
            }

            if (this.GameOver.ToString() != GameOver.None.ToString())
            {
                this.notificationService.SendGameOver(this.MovingPlayer, this.GameOver);
            }
        }

        private void IsPawnPromotion(string targetFen)
        {
            if (this.Move.Target.Piece.IsType(SymbolConstants.Pawn) &&
                this.Move.Target.Piece.IsLastMove)
            {
                this.Move.Target.Piece = Factory.GetQueen(this.MovingPlayer.Color);
                this.Move.Type = MoveType.PawnPromotion;
                this.utilityService.GetPawnPromotionFenString(targetFen, this.MovingPlayer, this.Move);
                this.ChessBoard.CalculateAttackedSquares();
            }
        }

        private void ChangeTurns()
        {
            if (this.Player1.HasToMove)
            {
                this.Player1.HasToMove = false;
                this.Player2.HasToMove = true;
            }
            else
            {
                this.Player2.HasToMove = false;
                this.Player1.HasToMove = true;
            }
        }

        private async Task UpdateHistory(Square oldSource, Square oldTarget, Board oldBoard)
        {
            var notation = this.utilityService
                .GetAlgebraicNotation(new AlgebraicNotationDto
                {
                    OldSource = oldSource,
                    OldTarget = oldTarget,
                    OldBoard = oldBoard,
                    Opponent = this.Opponent,
                    Turn = this.Turn,
                    Move = this.Move,
                });

            using var scope = this.serviceProvider.CreateScope();
            var moveRepository = scope.ServiceProvider.GetRequiredService<IRepository<MoveEntity>>();

            var onlyMoveNotation = notation.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
            await moveRepository.AddAsync(new MoveEntity
            {
                Notation = onlyMoveNotation,
                GameId = this.Id,
                UserId = this.MovingPlayer.UserId,
            });

            await moveRepository.SaveChangesAsync();

            this.notificationService.UpdateMoveHistory(this.MovingPlayer, notation);
        }
    }
}
