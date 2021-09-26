namespace Chess.Services.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Chess.Common.Enums;
    using Chess.Services.Data.Contracts;
    using Chess.Services.Data.Models.EventArgs;
    using Chess.Services.Data.Models.Pieces;

    public class Game
    {
        private readonly IDrawService drawService;
        private readonly ICheckService checkService;

        public Game(
            Player player1,
            Player player2,
            IDrawService drawService,
            ICheckService checkService)
        {
            this.drawService = drawService;
            this.checkService = checkService;

            this.Id = Guid.NewGuid().ToString();
            this.Player1 = player1;
            this.Player2 = player2;
            this.Player1.GameId = this.Id;
            this.Player2.GameId = this.Id;
            this.ChessBoard = Factory.GetBoard();
            this.Move = Factory.GetMove();
            this.GameOver = GameOver.None;
            this.Turn = 1;
        }

        public event EventHandler OnGameOver;

        public event EventHandler OnTakePiece;

        public event EventHandler OnMoveComplete;

        public event EventHandler OnMoveEvent;

        public event EventHandler OnThreefoldDrawAvailable;

        public string Id { get; set; }

        public Board ChessBoard { get; set; }

        public Move Move { get; set; }

        public GameOver GameOver { get; set; }

        public int Turn { get; set; }

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public Player MovingPlayer => this.Player1?.HasToMove ?? false ? this.Player1 : this.Player2;

        public Player Opponent => this.Player1?.HasToMove ?? false ? this.Player2 : this.Player1;

        public bool IsValidMove(string source, string target, string targetFen)
        {
            this.Move.Source = this.ChessBoard.GetSquareByName(source);
            this.Move.Target = this.ChessBoard.GetSquareByName(target);

            var oldSource = this.Move.Source.Clone() as Square;
            var oldTarget = this.Move.Target.Clone() as Square;
            var oldBoard = this.ChessBoard.Clone() as Board;
            var oldIsCheck = this.MovingPlayer.IsCheck;

            if (this.MovePiece() || this.TakePiece() || this.EnPassantTake())
            {
                this.IsPawnPromotion(targetFen);
                this.ClearCheckMessage();
                this.UpdateMoveHistory(oldSource, oldTarget, oldBoard);
                this.IsGameOver(targetFen);
                this.ChangeTurns();
                this.Turn++;
                return true;
            }
            else
            {
                this.InvalidMove(oldIsCheck);
                return false;
            }
        }

        public int CalculateRatingPoints(int yourRating, int opponentRating)
        {
            return Math.Max(1, ((opponentRating - yourRating) / 25) + 16);
        }

        public bool TryMove(Player player, Move move)
        {
            var oldPiece = move.Target.Piece;
            this.ChessBoard.ShiftPiece(move);
            this.ChessBoard.CalculateAttackedSquares();

            if (this.checkService.IsCheck(player, this.ChessBoard))
            {
                this.ChessBoard.ReversePiece(move, oldPiece);
                this.ChessBoard.CalculateAttackedSquares();
                return false;
            }

            if (player == this.Opponent)
            {
                this.ChessBoard.ReversePiece(move, oldPiece);
                this.ChessBoard.CalculateAttackedSquares();
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
                this.OnTakePiece?.Invoke(this.MovingPlayer, new TakePieceEventArgs(piece.Name, this.MovingPlayer.Points));
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
                    this.Move.Type = MoveType.Taking;
                    this.OnTakePiece?.Invoke(this.MovingPlayer, new TakePieceEventArgs(this.Move.Target.Piece.Name, this.MovingPlayer.Points));
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
            if (this.checkService.IsCheck(this.Opponent, this.ChessBoard))
            {
                this.OnMoveEvent?.Invoke(this.MovingPlayer, new MoveEventArgs(Message.CheckOpponent));
                if (this.checkService.IsCheckmate(this.ChessBoard, this.MovingPlayer, this.Opponent, this))
                {
                    this.GameOver = GameOver.Checkmate;
                }
            }

            this.MovingPlayer.IsThreefoldDrawAvailable = false;
            this.OnThreefoldDrawAvailable?.Invoke(this.MovingPlayer, new ThreefoldDrawEventArgs(false));

            if (this.drawService.IsThreefoldRepetionDraw(targetFen))
            {
                this.Opponent.IsThreefoldDrawAvailable = true;
                this.OnThreefoldDrawAvailable?.Invoke(this.MovingPlayer, new ThreefoldDrawEventArgs(true));
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
                this.OnGameOver?.Invoke(this.MovingPlayer, new GameOverEventArgs(this.GameOver));
            }
        }

        private void IsPawnPromotion(string targetFen)
        {
            if (this.Move.Target.Piece is Pawn && this.Move.Target.Piece.IsLastMove)
            {
                this.Move.Target.Piece = Factory.GetQueen(this.MovingPlayer.Color);
                this.Move.Type = MoveType.PawnPromotion;
                this.GetPawnPromotionFenString(targetFen);
                this.ChessBoard.CalculateAttackedSquares();
            }
        }

        private void InvalidMove(bool oldIsCheck)
        {
            if (this.MovingPlayer.IsCheck)
            {
                if (oldIsCheck)
                {
                    this.OnMoveEvent?.Invoke(this.MovingPlayer, new MoveEventArgs(Message.CheckSelf));
                }
                else
                {
                    this.OnMoveEvent?.Invoke(this.MovingPlayer, new MoveEventArgs(Message.CheckOpen));
                    this.MovingPlayer.IsCheck = false;
                }
            }
            else
            {
                this.OnMoveEvent?.Invoke(this.MovingPlayer, new MoveEventArgs(Message.InvalidMove));
            }
        }

        private void ClearCheckMessage()
        {
            if (!this.MovingPlayer.IsCheck && !this.Opponent.IsCheck)
            {
                this.OnMoveEvent?.Invoke(this.MovingPlayer, new MoveEventArgs(Message.CheckClear));
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

        private void GetPawnPromotionFenString(string targetFen)
        {
            var sb = new StringBuilder();
            string[] rows = targetFen.Split('/');

            var lastRow = this.MovingPlayer.Color == Color.White ? 0 : 7;
            var pawn = this.MovingPlayer.Color == Color.White ? "P" : "p";
            var queen = this.MovingPlayer.Color == Color.White ? "Q" : "q";

            for (int i = 0; i < rows.Length; i++)
            {
                var currentRow = rows[i];

                if (i == lastRow)
                {
                    for (int k = 0; k < currentRow.Length; k++)
                    {
                        var currentSymbol = currentRow[k].ToString();

                        if (string.Compare(currentSymbol, pawn) == 0)
                        {
                            sb.Append(queen);
                            continue;
                        }

                        sb.Append(currentSymbol);
                    }
                }
                else
                {
                    if (this.MovingPlayer.Color == Color.White)
                    {
                        sb.Append("/" + currentRow);
                    }
                    else
                    {
                        sb.Append(currentRow + "/");
                    }
                }
            }

            this.Move.PawnPromotionArgs.FenString = sb.ToString();
        }

        private void UpdateMoveHistory(Square source, Square target, Board board)
        {
            string notation = this.GetAlgebraicNotation(source, target, board);
            this.OnMoveComplete?.Invoke(this.MovingPlayer, new MoveCompleteEventArgs(notation));
        }

        private string GetAlgebraicNotation(Square source, Square target, Board board)
        {
            var sb = new StringBuilder();

            var playerTurn = Math.Ceiling(this.Turn / 2.0);
            sb.Append(playerTurn + ". ");

            if (this.Move.Type == MoveType.EnPassant)
            {
                var file = source.ToString()[0];
                sb.Append(file + "x" + target + "e.p");
            }
            else if (this.Move.Type == MoveType.Castling)
            {
                if (target.ToString()[0] == 'g')
                {
                    sb.Append("0-0");
                }
                else
                {
                    sb.Append("0-0-0");
                }
            }
            else if (this.Move.Type == MoveType.PawnPromotion)
            {
                sb.Append(target + "=Q");
            }
            else if (target.Piece == null || target.Piece.Color != source.Piece.Color)
            {
                if (source.Piece is Pawn)
                {
                    if (target.Piece == null)
                    {
                        sb.Append(target);
                    }
                    else
                    {
                        var file = source.ToString()[0];
                        sb.Append(file + "x" + target);
                    }
                }
                else
                {
                    var targetWithOldIsAttackedValue = board.Matrix.SelectMany(x => x).Where(y => y.Name == target.Name).FirstOrDefault();

                    if (targetWithOldIsAttackedValue.IsAttacked.Count(piece =>
                        piece.Color == source.Piece.Color &&
                        piece.Name.Contains(source.Piece.Name)) > 1)
                    {
                        var secondPieceSquare = board.Matrix.SelectMany(x => x).Where(square =>
                            square.Piece != null &&
                            square.Piece.Color == source.Piece.Color &&
                            square.Piece.Name.Contains(source.Piece.Name) &&
                            square.Name != source.Name).FirstOrDefault();

                        var xCheck = target.Piece == null ? string.Empty : "x";

                        if (secondPieceSquare.Position.File.Equals(source.Position.File))
                        {
                            var rank = source.Name[1];
                            if (target.Piece == null)
                            {
                                sb.Append($"{source.Piece.Symbol}{rank}{xCheck}{target}");
                            }
                            else
                            {
                                sb.Append($"{source.Piece.Symbol}{rank}{xCheck}{target}");
                            }
                        }
                        else
                        {
                            var file = source.Name[0];
                            sb.Append($"{source.Piece.Symbol}{file}{xCheck}{target}");
                        }
                    }
                    else
                    {
                        var check = target.Piece == null ? string.Empty : "x";
                        sb.Append($"{source.Piece.Symbol}{check}{target}");
                    }
                }
            }

            if (this.Opponent.IsCheck)
            {
                if (!this.Opponent.IsCheckMate)
                {
                    sb.Append("+");
                }
                else
                {
                    sb.Append("#");
                }
            }

            return sb.ToString();
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
    }
}
