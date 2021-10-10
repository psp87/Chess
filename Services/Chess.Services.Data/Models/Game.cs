namespace Chess.Services.Data.Models
{
    using System;
    using System.Collections.Generic;

    using Chess.Services.Data.Contracts;
    using Chess.Services.Data.Dtos;
    using Chess.Services.Data.Models.Pieces;
    using Common.Enums;

    public class Game
    {
        private readonly INotificationService notificationService;
        private readonly ICheckService checkService;
        private readonly IDrawService drawService;
        private readonly IUtilityService utilityService;

        public Game(
            Player player1,
            Player player2,
            INotificationService notificationService,
            ICheckService checkService,
            IDrawService drawService,
            IUtilityService utilityService)
        {
            this.notificationService = notificationService;
            this.checkService = checkService;
            this.drawService = drawService;
            this.utilityService = utilityService;

            this.Player1 = player1;
            this.Player2 = player2;
            this.Player1.GameId = this.Id;
            this.Player2.GameId = this.Id;
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
                this.notificationService
                    .ClearCheck(this.MovingPlayer, this.Opponent);
                this.checkService
                    .IsCheck(this.Opponent, this.ChessBoard);
                this.UpdateHistory(oldSource, oldTarget, oldBoard);
                this.IsGameOver(targetFen);
                this.ChangeTurns();
                this.Turn++;

                return true;
            }

            this.notificationService
                .InvalidMove(oldIsCheck, this.MovingPlayer);

            return false;
        }

        public bool TryMove(Player player, Move move)
        {
            var oldPiece = move.Target.Piece;
            this.ChessBoard
                .ShiftPiece(move);
            this.ChessBoard
                .CalculateAttackedSquares();

            if (this.checkService.IsCheck(player, this.ChessBoard))
            {
                this.ChessBoard
                    .ReversePiece(move, oldPiece);
                this.ChessBoard
                    .CalculateAttackedSquares();

                return false;
            }

            if (player == this.Opponent)
            {
                this.ChessBoard
                    .ReversePiece(move, oldPiece);
                this.ChessBoard
                    .CalculateAttackedSquares();
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
                this.notificationService
                    .UpdateTakenPiecesHistory(this.MovingPlayer, piece.Name);

                return true;
            }

            return false;
        }

        private bool EnPassantTake()
        {
            if (this.Move.EnPassantArgs.SquareAvailable != null)
            {
                var positions = this.GetAllowedPositions();

                if (this.Move.EnPassantArgs.Turn == this.Turn &&
                    this.Move.EnPassantArgs.SquareAvailable.Equals(this.Move.Target) &&
                    this.Move.Source.Piece is Pawn &&
                    (this.Move.Source.Position.Equals(positions[0]) ||
                    this.Move.Source.Position.Equals(positions[1])))
                {
                    if (!this.TryEnPassantMove())
                    {
                        this.MovingPlayer.IsCheck = true;
                        return false;
                    }

                    this.MovingPlayer.IsCheck = false;
                    this.MovingPlayer.TakeFigure(this.Move.Target.Piece.Name);
                    this.MovingPlayer.Points += this.Move.Target.Piece.Points;
                    //this.Move.Type = MoveType.Taking;

                    this.notificationService
                        .UpdateTakenPiecesHistory(this.MovingPlayer, this.Move.Target.Piece.Name);
                    this.Move.EnPassantArgs.SquareAvailable = null;

                    return true;
                }
            }

            return false;
        }

        private bool TryEnPassantMove()
        {
            int offsetX = this.Move.Target.Position.File > this.Move.Source.Position.File ? 1 : -1;
            this.ChessBoard.ShiftEnPassant(this.Move, offsetX);
            this.ChessBoard.CalculateAttackedSquares();

            if (this.checkService.IsCheck(this.MovingPlayer, this.ChessBoard))
            {
                this.ChessBoard.ReverseEnPassant(this.Move, offsetX);
                this.ChessBoard.CalculateAttackedSquares();
                return false;
            }

            var square = this.ChessBoard.GetSquareByCoordinates(this.Move.Source.Position.Rank, this.Move.Source.Position.File + offsetX);
            this.Move.EnPassantArgs.SquareTakenPiece = square;
            this.Move.Type = MoveType.EnPassant;
            return true;
        }

        private void IsGameOver(string targetFen)
        {
            if (this.checkService
                .IsCheck(this.Opponent, this.ChessBoard))
            {
                this.notificationService
                    .SendCheck(this.MovingPlayer);

                if (this.checkService
                    .IsCheckmate(this.ChessBoard, this.MovingPlayer, this.Opponent, this))
                {
                    this.GameOver = GameOver.Checkmate;
                }
            }

            this.MovingPlayer.IsThreefoldDrawAvailable = false;
            this.notificationService
                .SendThreefoldDrawAvailability(this.MovingPlayer, false);

            if (this.drawService
                .IsThreefoldRepetionDraw(targetFen))
            {
                this.Opponent.IsThreefoldDrawAvailable = true;
                this.notificationService
                    .SendThreefoldDrawAvailability(this.MovingPlayer, true);
            }

            if (this.drawService
                .IsFivefoldRepetitionDraw(targetFen))
            {
                this.GameOver = GameOver.FivefoldDraw;
            }

            if (this.drawService
                .IsFiftyMoveDraw(this.Move))
            {
                this.GameOver = GameOver.FiftyMoveDraw;
            }

            if (this.drawService
                .IsDraw(this.ChessBoard))
            {
                this.GameOver = GameOver.Draw;
            }

            if (this.drawService
                .IsStalemate(this.ChessBoard, this.Opponent))
            {
                this.GameOver = GameOver.Stalemate;
            }

            if (this.GameOver.ToString() != GameOver.None.ToString())
            {
                this.notificationService
                    .SendGameOver(this.MovingPlayer, this.GameOver);
            }
        }

        private void IsPawnPromotion(string targetFen)
        {
            if (this.Move.Target.Piece is Pawn && this.Move.Target.Piece.IsLastMove)
            {
                this.Move.Target.Piece = Factory.GetQueen(this.MovingPlayer.Color);
                this.Move.Type = MoveType.PawnPromotion;
                this.utilityService
                    .GetPawnPromotionFenString(targetFen, this.MovingPlayer, this.Move);
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

        private List<Position> GetAllowedPositions()
        {
            var positions = new List<Position>();

            var offsetPlayer = this.MovingPlayer.Color == Color.White ? 1 : -1;

            int rank = this.Move.Target.Position.Rank + offsetPlayer;
            int fileFirst = this.Move.Target.Position.File + 1;
            int fileSecond = this.Move.Target.Position.File - 1;

            var firstAllowedPosition = Factory.GetPosition(rank, fileFirst);
            var secondAllowedPosition = Factory.GetPosition(rank, fileSecond);

            positions.Add(firstAllowedPosition);
            positions.Add(secondAllowedPosition);

            return positions;
        }

        private void UpdateHistory(Square oldSource, Square oldTarget, Board oldBoard)
        {
            var notation = this.utilityService
                    .GetAlgebraicNotation(
                    new AlgebraicNotationDto
                    {
                        OldSource = oldSource,
                        OldTarget = oldTarget,
                        OldBoard = oldBoard,
                        Opponent = this.Opponent,
                        Turn = this.Turn,
                        Move = this.Move,
                    });

            this.notificationService.UpdateMoveHistory(this.MovingPlayer, notation);
        }
    }
}
