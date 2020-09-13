namespace Chess.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Chess.Common;
    using Chess.Common.Enums;
    using Chess.Data.Models.EventArgs;
    using Chess.Data.Models.Pieces;
    using Chess.Data.Models.Pieces.Contracts;

    public class Game
    {
        private Queue<string> movesThreefold;
        private Queue<string> movesFivefold;
        private string[] arrayThreefold;
        private string[] arrayFivefold;

        public Game(Player player1, Player player2)
        {
            this.ChessBoard = Factory.GetBoard();

            this.Player1 = player1;
            this.Player2 = player2;

            this.Id = Guid.NewGuid().ToString();
            this.Player1.GameId = this.Id;
            this.Player2.GameId = this.Id;
            this.Move = Factory.GetMove();

            this.GameOver = GameOver.None;
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
            this.Move.Source = this.ChessBoard.GetSquareByName(source);
            this.Move.Target = this.ChessBoard.GetSquareByName(target);

            var oldSource = this.Move.Source.Clone() as Square;
            var oldTarget = this.Move.Target.Clone() as Square;

            if (this.MovePiece() || this.TakePiece() || this.EnPassantTake())
            {
                this.IsPawnPromotion(targetFen);
                this.IsGameOver(targetFen);
                this.ClearCheckMessage();

                string notation = this.GetAlgebraicNotation(oldSource, oldTarget);
                this.OnMoveComplete?.Invoke(this.MovingPlayer, new NotationEventArgs(notation));

                this.Turn++;
                this.ChangeTurns();
                return true;
            }
            else
            {
                if (this.MovingPlayer.IsCheck)
                {
                    this.OnNotification?.Invoke(this.MovingPlayer, new MessageEventArgs(Notification.CheckSelf));
                }
                else
                {
                    this.OnNotification?.Invoke(this.MovingPlayer, new MessageEventArgs(Notification.InvalidMove));
                }

                this.MovingPlayer.IsCheck = false;
                return false;
            }
        }

        private bool IsCheckmate()
        {
            if (!this.IsKingAbleToMove() && !this.AttackingPieceCanBeTaken() && !this.OtherPieceCanBlockTheCheck())
            {
                this.Opponent.IsCheckMate = true;
                return true;
            }

            return false;
        }

        private bool IsStalemate()
        {
            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                for (int x = 0; x < GlobalConstants.BoardCols; x++)
                {
                    var currentFigure = this.ChessBoard.GetSquareByCoordinates(y, x).Piece;

                    if (currentFigure != null && currentFigure.Color == this.Opponent.Color)
                    {
                        currentFigure.IsMoveAvailable(this.ChessBoard.Matrix);
                        if (currentFigure.IsMovable)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool IsDraw()
        {
            int counterBishopKnightWhite = 0;
            int counterBishopKnightBlack = 0;

            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                for (int x = 0; x < GlobalConstants.BoardCols; x++)
                {
                    var currentFigure = this.ChessBoard.GetSquareByCoordinates(y, x).Piece;

                    if (!(currentFigure == null || currentFigure is King))
                    {
                        if (currentFigure is Pawn ||
                            currentFigure is Rook ||
                            currentFigure is Queen ||
                            counterBishopKnightWhite > 1 ||
                            counterBishopKnightBlack > 1)
                        {
                            return false;
                        }

                        if (currentFigure.Color == Color.White)
                        {
                            counterBishopKnightWhite++;
                        }
                        else
                        {
                            counterBishopKnightBlack++;
                        }
                    }
                }
            }

            return true;
        }

        private void IsThreefoldRepetionDraw(string fen)
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

        private bool IsFivefoldRepetitionDraw(string fen)
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

        private void IsGameOver(string targetFen)
        {
            if (this.IsPlayerChecked(this.Opponent))
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

            if (this.IsDraw())
            {
                this.GameOver = GameOver.Draw;
            }

            if (this.IsStalemate())
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

        private void ClearCheckMessage()
        {
            if (!this.MovingPlayer.IsCheck && !this.Opponent.IsCheck)
            {
                this.OnNotification?.Invoke(this.MovingPlayer, new MessageEventArgs(Notification.CheckClear));
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

        private bool MovePiece()
        {
            if (this.Move.Target.Piece == null &&
                this.MovingPlayer.Color == this.Move.Source.Piece.Color &&
                this.Move.Source.Piece.Move(this.Move.Target.Position, this.ChessBoard.Matrix, this.Turn, this.Move))
            {
                if (!this.Try())
                {
                    this.MovingPlayer.IsCheck = true;
                    return false;
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
                    return false;
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

                    this.ChessBoard.ShiftEnPassant(this.Move, x);
                    this.ChessBoard.CalculateAttackedSquares();

                    if (this.IsPlayerChecked(this.MovingPlayer))
                    {
                        this.ChessBoard.ReverseEnPassant(this.Move, x);
                        this.ChessBoard.CalculateAttackedSquares();

                        this.MovingPlayer.IsCheck = true;
                        return false;
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
            this.ChessBoard.ShiftPiece(this.Move);
            this.ChessBoard.CalculateAttackedSquares();

            if (this.IsPlayerChecked(this.MovingPlayer))
            {
                this.ChessBoard.ReversePiece(this.Move);
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

        private bool IsPlayerChecked(Player player)
        {
            var kingSquare = this.ChessBoard.GetKingSquare(player.Color);

            if (kingSquare.IsAttacked.Where(x => x.Color != kingSquare.Piece.Color).Any())
            {
                player.IsCheck = true;
                return true;
            }

            return false;
        }

        private bool IsKingAbleToMove()
        {
            var king = this.ChessBoard.GetKingSquare(this.Opponent.Color);

            int kingY = king.Position.Y;
            int kingX = king.Position.X;

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (y == 0 && x == 0)
                    {
                        continue;
                    }

                    if (Position.IsInBoard(kingY + y, kingX + x))
                    {
                        var checkedSquare = this.ChessBoard.Matrix[kingY + y][kingX + x];

                        if (this.NeighbourSquareAvailable(checkedSquare))
                        {
                            var currentFigure = this.ChessBoard.Matrix[kingY][kingX].Piece;
                            var neighbourFigure = this.ChessBoard.Matrix[kingY + y][kingX + x].Piece;

                            this.AssignNewValuesAndCalculate(kingY, kingX, y, x, currentFigure);

                            if (!this.ChessBoard.Matrix[kingY + y][kingX + x].IsAttacked.Where(k => k.Color == this.MovingPlayer.Color).Any())
                            {
                                this.AssignOldValuesAndCalculate(kingY, kingX, y, x, currentFigure, neighbourFigure);
                                return true;
                            }

                            this.AssignOldValuesAndCalculate(kingY, kingX, y, x, currentFigure, neighbourFigure);
                        }
                    }
                }
            }

            return false;
        }

        private bool AttackingPieceCanBeTaken()
        {
            var opponent = this.MovingPlayer;

            if (this.Move.Target.IsAttacked.Where(x => x.Color == opponent.Color).Any())
            {
                if (this.Move.Target.IsAttacked.Count(x => x.Color == opponent.Color) > 1)
                {
                    return true;
                }
                else if (!(this.Move.Target.IsAttacked.Where(x => x.Color == opponent.Color).First() is King))
                {
                    return true;
                }
            }

            return false;
        }

        private bool OtherPieceCanBlockTheCheck()
        {
            var king = this.ChessBoard.GetKingSquare(this.Opponent.Color);

            if (!(this.Move.Target.Piece is Knight) && !(this.Move.Target.Piece is Pawn))
            {
                int kingY = king.Position.Y;
                int kingX = king.Position.X;

                int attackingRow = this.Move.Target.Position.Y;
                int attackingCol = this.Move.Target.Position.X;

                if (attackingRow == kingY)
                {
                    int difference = Math.Abs(attackingCol - kingX) - 1;

                    for (int i = 1; i <= difference; i++)
                    {
                        int sign = attackingCol - kingX < 0 ? i : -i;
                        var signPlayer = this.Opponent.Color == Color.White ? 1 : -1;

                        var currentSquare = this.ChessBoard.Matrix[kingY][attackingCol + sign];
                        var neighbourSquare = this.ChessBoard.Matrix[kingY][attackingCol + sign + signPlayer];

                        if (currentSquare.IsAttacked.Where(x => x.Color == this.Opponent.Color && !(x is King) && !(x is Pawn)).Any() ||
                            (neighbourSquare.Piece is Pawn && neighbourSquare.Piece.Color == this.Opponent.Color))
                        {
                            return true;
                        }
                    }
                }

                if (attackingCol == kingX)
                {
                    int difference = Math.Abs(attackingRow - kingY) - 1;

                    for (int i = 1; i <= difference; i++)
                    {
                        int sign = attackingRow - kingY < 0 ? i : -i;

                        if (this.ChessBoard.Matrix[attackingRow + sign][kingX].IsAttacked.Where(x => x.Color == this.Opponent.Color && !(x is King) && !(x is Pawn)).Any())
                        {
                            return true;
                        }
                    }
                }

                if (attackingRow != kingY && attackingCol != kingX)
                {
                    int difference = Math.Abs(attackingRow - kingY) - 1;

                    for (int i = 1; i <= difference; i++)
                    {
                        int signRow = attackingRow - kingY < 0 ? i : -i;
                        int signCol = attackingCol - kingX < 0 ? i : -i;
                        var signPlayer = this.Opponent.Color == Color.White ? 1 : -1;

                        var currentSquare = this.ChessBoard.Matrix[attackingRow + signRow][attackingCol + signCol];
                        var neighbourSquare = this.ChessBoard.Matrix[attackingRow + signRow + signPlayer][attackingCol + signCol];

                        if (currentSquare.IsAttacked.Where(x => x.Color == this.Opponent.Color && !(x is King) && !(x is Pawn)).Any() ||
                            (neighbourSquare.Piece is Pawn && neighbourSquare.Piece.Color == this.Opponent.Color))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool NeighbourSquareAvailable(Square square)
        {
            if (square.Piece != null &&
                square.Piece.Color == this.MovingPlayer.Color &&
                !square.IsAttacked.Where(x => x.Color == this.MovingPlayer.Color).Any())
            {
                return true;
            }

            if (square.Piece == null &&
                !square.IsAttacked.Where(x => x.Color == this.MovingPlayer.Color).Any())
            {
                return true;
            }

            return false;
        }

        private void AssignNewValuesAndCalculate(int kingRow, int kingCol, int i, int k, IPiece currentFigure)
        {
            this.ChessBoard.Matrix[kingRow][kingCol].Piece = null;
            this.ChessBoard.Matrix[kingRow + i][kingCol + k].Piece = currentFigure;
            this.ChessBoard.CalculateAttackedSquares();
        }

        private void AssignOldValuesAndCalculate(int kingRow, int kingCol, int i, int k, IPiece currentFigure, IPiece neighbourFigure)
        {
            this.ChessBoard.Matrix[kingRow][kingCol].Piece = currentFigure;
            this.ChessBoard.Matrix[kingRow + i][kingCol + k].Piece = neighbourFigure;
            this.ChessBoard.CalculateAttackedSquares();
        }
    }
}
