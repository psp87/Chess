namespace Chess.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Chess.Common.Enums;
    using Chess.Data.Models.EventArgs;
    using Chess.Data.Models.Pieces;

    public class Game
    {
        private Queue<string> movesThreefold;
        private Queue<string> movesFivefold;
        private string[] arrayThreefold;
        private string[] arrayFivefold;

        public Game(Player player1, Player player2)
        {
            this.ChessBoard = Factory.GetBoard();
            this.ChessBoard.Initialize();

            this.Player1 = player1;
            this.Player2 = player2;

            this.Id = Guid.NewGuid().ToString();
            this.GameOver = GameOver.None;
            this.Player1.GameId = this.Id;
            this.Player2.GameId = this.Id;
            this.Move = Factory.GetMove();
            this.Move.Type = MoveType.Normal;
            this.Turn = 1;

            this.movesThreefold = new Queue<string>();
            this.movesFivefold = new Queue<string>();
            this.arrayThreefold = new string[9];
            this.arrayFivefold = new string[17];

        }

        public event EventHandler OnNotification;

        public event EventHandler OnGameOver;

        public event EventHandler OnMoveComplete;

        public event EventHandler OnTakePiece;

        public event EventHandler OnThreefoldDrawAvailable;

        public string Id { get; set; }

        public Move Move { get; set; }

        public Board ChessBoard { get; set; }

        public GameOver GameOver { get; set; }

        public int Turn { get; set; }

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public Player MovingPlayer => this.Player1?.HasToMove ?? false ? this.Player1 : this.Player2;

        public Player Opponent => this.Player1?.HasToMove ?? false ? this.Player2 : this.Player1;

        public bool MakeMove(string source, string target, string targetFen)
        {
            this.Move.Source = this.ChessBoard.GetSquare(source);
            this.Move.Target = this.ChessBoard.GetSquare(target);

            var oldSource = this.Move.Source.Clone() as Square;
            var oldTarget = this.Move.Target.Clone() as Square;

            if (this.MovePiece() ||
                this.TakePiece() ||
                this.EnPassantTake())
            {
                this.IsPawnPromotion(targetFen);

                if (this.MovingPlayer.IsCheck)
                {
                    this.OnNotification?.Invoke(this.MovingPlayer, new MessageEventArgs(Notification.CheckSelf));
                    return false;
                }

                this.IsGameOver(targetFen);
                this.ClearCheckMessage();

                string notation = this.GetAlgebraicNotation(oldSource, oldTarget);
                this.OnMoveComplete?.Invoke(this.MovingPlayer, new NotationEventArgs(notation));

                this.Turn++;
                this.ChangeTurns();

                return true;
            }

            if (!this.MovingPlayer.IsCheck)
            {
                this.OnNotification?.Invoke(this.MovingPlayer, new MessageEventArgs(Notification.InvalidMove));
            }

            this.MovingPlayer.IsCheck = false;

            return false;
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

        private void ClearCheckMessage()
        {
            if (!this.MovingPlayer.IsCheck && !this.Opponent.IsCheck)
            {
                this.OnNotification?.Invoke(this.MovingPlayer, new MessageEventArgs(Notification.CheckClear));
            }
        }

        private void IsGameOver(string targetFen)
        {
            if (this.ChessBoard.IsPlayerChecked(this.Opponent))
            {
                this.OnNotification?.Invoke(this.Opponent, new MessageEventArgs(Notification.CheckOpponent));
                if (this.IsCheckmate())
                {
                    this.GameOver = GameOver.Checkmate;
                }
            }

            this.IsThreefoldRepetionDraw(targetFen);

            if (this.IsFivefoldRepetitionDraw(targetFen))
            {
                this.GameOver = GameOver.FivefoldDraw;
            }

            if (this.ChessBoard.IsDraw())
            {
                this.GameOver = GameOver.Draw;
            }

            if (this.ChessBoard.IsStalemate(this.Opponent))
            {
                this.GameOver = GameOver.Stalemate;
            }

            if (this.GameOver.ToString() != GameOver.None.ToString())
            {
                this.OnGameOver?.Invoke(this.MovingPlayer, new GameOverEventArgs(this.GameOver));
            }
        }

        public bool IsCheckmate()
        {
            var king = this.ChessBoard.GetKingSquare(this.Opponent.Color);

            if (!this.ChessBoard.IsKingAbleToMove(king, this.MovingPlayer) &&
                !this.ChessBoard.AttackingPieceCanBeTaken(this.Move.Target, this.MovingPlayer) &&
                !this.ChessBoard.OtherPieceCanBlockTheCheck(king, this.Move.Target, this.Opponent))
            {
                this.Opponent.IsCheckMate = true;
                return true;
            }

            return false;
        }

        public void IsThreefoldRepetionDraw(string fen)
        {
            this.movesThreefold.Enqueue(fen);

            this.MovingPlayer.IsThreefoldDrawAvailable = false;
            this.OnThreefoldDrawAvailable?.Invoke(this.MovingPlayer, new ThreefoldDrawEventArgs(false));

            if (this.movesThreefold.Count == 9)
            {
                this.arrayThreefold = this.movesThreefold.ToArray();

                var isFirstFenSame = string.Compare(fen, this.arrayThreefold[0]) == 0;
                var isFiveFenSame = string.Compare(fen, this.arrayThreefold[4]) == 0;

                if (isFirstFenSame && isFiveFenSame)
                {
                    this.Opponent.IsThreefoldDrawAvailable = true;
                    this.OnThreefoldDrawAvailable?.Invoke(this.MovingPlayer, new ThreefoldDrawEventArgs(true));
                }

                this.movesThreefold.Dequeue();
            }
        }

        public bool IsFivefoldRepetitionDraw(string fen)
        {
            this.movesFivefold.Enqueue(fen);

            if (this.movesFivefold.Count == 17)
            {
                this.arrayFivefold = this.movesFivefold.ToArray();

                var isFirstFenSame = string.Compare(fen, this.arrayFivefold[0]) == 0;
                var isFiveFenSame = string.Compare(fen, this.arrayFivefold[4]) == 0;
                var isNineFenSame = string.Compare(fen, this.arrayFivefold[8]) == 0;
                var isThirteenFenSame = string.Compare(fen, this.arrayFivefold[12]) == 0;

                if (isFirstFenSame && isFiveFenSame && isNineFenSame && isThirteenFenSame)
                {
                    return true;
                }
                else
                {
                    this.movesFivefold.Dequeue();
                }
            }

            return false;
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

        private bool MovePiece()
        {
            if (this.Move.Target.Piece == null &&
                this.MovingPlayer.Color == this.Move.Source.Piece.Color &&
                this.Move.Source.Piece.Move(this.Move.Target.Position, this.ChessBoard.Matrix, this.Turn, this.Move))
            {
                if (!this.Try())
                {
                    this.MovingPlayer.IsCheck = true;
                    return true;
                }

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

                if (!this.Try())
                {
                    this.MovingPlayer.IsCheck = true;
                    return true;
                }

                this.Move.Target.Piece.IsFirstMove = false;
                this.MovingPlayer.TakeFigure(piece.Name);
                this.MovingPlayer.Points += piece.Points;
                this.OnTakePiece?.Invoke(this.MovingPlayer, new TakePieceEventArgs(piece.Name, this.MovingPlayer.Points));
                return true;
            }

            return false;
        }

        private bool EnPassantTake()
        {
            if (this.Move.EnPassantArgs.Position != null)
            {
                var positions = this.GetAllowedPositions();

                var firstPosition = positions[0];
                var secondPosition = positions[1];

                if (this.Move.EnPassantArgs.Turn == this.Turn &&
                    this.Move.EnPassantArgs.Position.Equals(this.Move.Target.Position) &&
                    this.Move.Source.Piece is Pawn &&
                    (this.Move.Source.Position.Equals(firstPosition) ||
                    this.Move.Source.Position.Equals(secondPosition)))
                {
                    var piece = Factory.GetPawn(this.MovingPlayer.Color);
                    int x = this.Move.Target.Position.X > this.Move.Source.Position.X ? 1 : -1;

                    this.ChessBoard.ShiftEnPassant(x, this.Move);
                    this.ChessBoard.CalculateAttackedSquares();

                    if (this.ChessBoard.IsPlayerChecked(this.MovingPlayer))
                    {
                        this.ChessBoard.ReverseEnPassant(x, this.Move);
                        this.ChessBoard.CalculateAttackedSquares();

                        this.MovingPlayer.IsCheck = true;
                        return true;
                    }

                    string position = this.GetStringPosition(this.Move.Source.Position.X + x, this.Move.Source.Position.Y);
                    this.Move.EnPassantArgs.FenString = position;
                    this.Move.Type = MoveType.EnPassant;

                    this.MovingPlayer.TakeFigure(piece.Name);
                    this.MovingPlayer.Points += piece.Points;
                    this.OnTakePiece?.Invoke(this.MovingPlayer, new TakePieceEventArgs(piece.Name, this.MovingPlayer.Points));
                    this.MovingPlayer.IsCheck = false;
                    return true;
                }
            }

            return false;
        }

        private bool Try()
        {
            this.ChessBoard.ShiftPiece(this.Move.Source, this.Move.Target);
            this.ChessBoard.CalculateAttackedSquares();

            if (this.ChessBoard.IsPlayerChecked(this.MovingPlayer))
            {
                this.ChessBoard.Reverse(this.Move.Source, this.Move.Target);
                this.ChessBoard.CalculateAttackedSquares();
                return false;
            }

            this.MovingPlayer.IsCheck = false;
            return true;
        }

        private List<Position> GetAllowedPositions()
        {
            var positions = new List<Position>();

            var sign = this.MovingPlayer.Color == Color.White ? 1 : -1;

            int row = this.Move.Target.Position.Y + sign;
            int colFirst = this.Move.Target.Position.X + 1;
            int colSecond = this.Move.Target.Position.X - 1;

            var firstAllowedPosition = Factory.GetPosition(row, colFirst);
            var secondAllowedPosition = Factory.GetPosition(row, colSecond);

            positions.Add(firstAllowedPosition);
            positions.Add(secondAllowedPosition);

            return positions;
        }

        private string GetStringPosition(int x, int y)
        {
            char col = (char)(97 + x);
            int row = Math.Abs(y - 8);
            return $"{col}{row}";
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

        private string GetAlgebraicNotation(Square source, Square target)
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
            else if (target.Piece == null)
            {
                if (source.Piece is Pawn)
                {
                    sb.Append(target);
                }
                else
                {
                    sb.Append(source.Piece.Symbol + target.ToString());
                }
            }
            else if (target.Piece.Color != source.Piece.Color)
            {
                if (source.Piece is Pawn)
                {
                    var file = source.ToString()[0];
                    sb.Append(file + "x" + target);
                }
                else
                {
                    sb.Append(source.Piece.Symbol + "x" + target);
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
    }
}
