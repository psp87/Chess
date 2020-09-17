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

    public class Game
    {
        private readonly Queue<string> movesThreefold;
        private readonly Queue<string> movesFivefold;
        private string[] arrayThreefold;
        private string[] arrayFivefold;

        public Game(Player player1, Player player2)
        {
            this.Id = Guid.NewGuid().ToString();
            this.ChessBoard = Factory.GetBoard();

            this.Player1 = player1;
            this.Player2 = player2;
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

        public bool MakeMove(string source, string target, string targetFen)
        {
            this.Move.Source = this.ChessBoard.GetSquareByName(source);
            this.Move.Target = this.ChessBoard.GetSquareByName(target);

            var oldSource = this.Move.Source.Clone() as Square;
            var oldTarget = this.Move.Target.Clone() as Square;
            var oldBoard = this.ChessBoard.Clone() as Board;

            if (this.MovePiece() || this.TakePiece() || this.EnPassantTake())
            {
                this.IsPawnPromotion(targetFen);
                this.IsGameOver(targetFen);
                this.ClearCheckMessage();
                this.UpdateMoveHistory(oldSource, oldTarget, oldBoard);
                this.ChangeTurns();
                this.Turn++;
                return true;
            }
            else
            {
                if (this.MovingPlayer.IsCheck)
                {
                    this.OnMoveEvent?.Invoke(this.MovingPlayer, new MoveEventArgs(Notification.CheckSelf));
                }
                else
                {
                    this.OnMoveEvent?.Invoke(this.MovingPlayer, new MoveEventArgs(Notification.InvalidMove));
                }

                this.MovingPlayer.IsCheck = false;
                return false;
            }
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
                    this.OnTakePiece?.Invoke(this.MovingPlayer, new TakePieceEventArgs(this.Move.Target.Piece.Name, this.MovingPlayer.Points));
                    this.Move.EnPassantArgs.SquareAvailable = null;
                    return true;
                }
            }

            return false;
        }

        private bool TryMove(Player player, Move move)
        {
            this.ChessBoard.ShiftPiece(move);
            this.ChessBoard.CalculateAttackedSquares();

            if (this.IsPlayerChecked(player))
            {
                this.ChessBoard.ReversePiece(move);
                this.ChessBoard.CalculateAttackedSquares();
                return false;
            }

            return true;
        }

        private bool TryEnPassantMove()
        {
            int offsetX = this.Move.Target.Position.File > this.Move.Source.Position.File ? 1 : -1;
            this.ChessBoard.ShiftEnPassant(this.Move, offsetX);
            this.ChessBoard.CalculateAttackedSquares();

            if (this.IsPlayerChecked(this.MovingPlayer))
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
            if (this.IsPlayerChecked(this.Opponent))
            {
                this.OnMoveEvent?.Invoke(this.Opponent, new MoveEventArgs(Notification.CheckOpponent));
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

        private bool IsCheckmate()
        {
            if (!this.IsKingAbleToMove() && !this.IsAbleToTakeAttackingPiece() && !this.CanOtherPieceBlockTheCheck())
            {
                this.Opponent.IsCheckMate = true;
                return true;
            }

            return false;
        }

        private bool IsStalemate()
        {
            for (int rank = 0; rank < GlobalConstants.Ranks; rank++)
            {
                for (int file = 0; file < GlobalConstants.Files; file++)
                {
                    var currentFigure = this.ChessBoard.GetSquareByCoordinates(rank, file).Piece;

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

            for (int rank = 0; rank < GlobalConstants.Ranks; rank++)
            {
                for (int file = 0; file < GlobalConstants.Files; file++)
                {
                    var currentFigure = this.ChessBoard.GetSquareByCoordinates(rank, file).Piece;

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
                this.OnMoveEvent?.Invoke(this.MovingPlayer, new MoveEventArgs(Notification.CheckClear));
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

        private bool IsKingAbleToMove()
        {
            var opponentKingSquare = this.ChessBoard.GetKingSquare(this.Opponent.Color);

            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    if (offsetY == 0 && offsetX == 0)
                    {
                        continue;
                    }

                    var rank = opponentKingSquare.Position.Rank + offsetY;
                    var file = opponentKingSquare.Position.File + offsetX;

                    if (Position.IsInBoard(file, rank))
                    {
                        var checkedSquare = this.ChessBoard.GetSquareByCoordinates(rank, file);

                        if (this.IsSquareAvailable(checkedSquare))
                        {
                            var move = Factory.GetMove(opponentKingSquare, checkedSquare);

                            if (this.TryMove(this.Opponent, move))
                            {
                                this.ChessBoard.ReversePiece(move);
                                this.ChessBoard.CalculateAttackedSquares();
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool IsAbleToTakeAttackingPiece()
        {
            if (this.Move.Target.IsAttacked.Where(x => x.Color == this.Opponent.Color).Any())
            {
                if (this.Move.Target.IsAttacked.Count(x => x.Color == this.Opponent.Color) > 1 ||
                    !(this.Move.Target.IsAttacked.Where(x => x.Color == this.Opponent.Color).First() is King))
                {
                    var defendPieces = this.Move.Target.IsAttacked.Where(x => x.Color == this.Opponent.Color).ToArray();

                    for (int i = 0; i < defendPieces.Length; i++)
                    {
                        var currentDefendPosition = defendPieces[i].Position;
                        var currentDefendSquare = this.ChessBoard.Matrix.SelectMany(x => x).Where(y => y.Position.Equals(currentDefendPosition)).FirstOrDefault();
                        var move = Factory.GetMove(currentDefendSquare, this.Move.Target);

                        if (this.TryMove(this.Opponent, move))
                        {
                            this.ChessBoard.ReversePiece(move);
                            this.ChessBoard.CalculateAttackedSquares();
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool CanOtherPieceBlockTheCheck()
        {
            var opponentKingSquare = this.ChessBoard.GetKingSquare(this.Opponent.Color);

            if (!(this.Move.Target.Piece is Knight) && !(this.Move.Target.Piece is Pawn))
            {
                int kingRank = opponentKingSquare.Position.Rank;
                int kingFile = opponentKingSquare.Position.File;

                int attackRank = this.Move.Target.Position.Rank;
                int attackFile = this.Move.Target.Position.File;

                if (attackRank == kingRank)
                {
                    int squaresBetween = Math.Abs(attackFile - kingFile) - 1;

                    for (int i = 1; i <= squaresBetween; i++)
                    {
                        int offsetX = attackFile - kingFile < 0 ? i : -i;
                        var offsetPlayer = this.Opponent.Color == Color.White ? 1 : -1;

                        var currentSquare = this.ChessBoard.GetSquareByCoordinates(kingRank, attackFile + offsetX);

                        // Check offsetPlayer?
                        var neighbourSquare = this.ChessBoard.GetSquareByCoordinates(kingRank, attackFile + offsetX + offsetPlayer);

                        if (currentSquare.IsAttacked.Where(x => x.Color == this.Opponent.Color && !(x is King) && !(x is Pawn)).Any() ||
                           (neighbourSquare.Piece is Pawn && neighbourSquare.Piece.Color == this.Opponent.Color))
                        {
                            return true;
                        }
                    }
                }

                if (attackFile == kingFile)
                {
                    int squaresBetween = Math.Abs(attackRank - kingRank) - 1;

                    for (int i = 1; i <= squaresBetween; i++)
                    {
                        int offsetY = attackRank - kingRank < 0 ? i : -i;
                        var checkedSquare = this.ChessBoard.GetSquareByCoordinates(attackRank + offsetY, kingFile);

                        if (checkedSquare.IsAttacked.Where(x => x.Color == this.Opponent.Color &&
                            !(x is King) && !(x is Pawn)).Any())
                        {
                            return true;
                        }
                    }
                }

                if (attackRank != kingRank && attackFile != kingFile)
                {
                    int squaresBetween = Math.Abs(attackRank - kingRank) - 1;

                    for (int i = 1; i <= squaresBetween; i++)
                    {
                        int offsetY = attackRank - kingRank < 0 ? i : -i;
                        int offsetX = attackFile - kingFile < 0 ? i : -i;
                        var offsetPlayer = this.Opponent.Color == Color.White ? 1 : -1;

                        var currentSquare = this.ChessBoard.GetSquareByCoordinates(attackRank + offsetY, attackFile + offsetX);
                        var neighbourSquare = this.ChessBoard.GetSquareByCoordinates(attackRank + offsetY + offsetPlayer, attackFile + offsetX);

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

        private bool IsSquareAvailable(Square square)
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
