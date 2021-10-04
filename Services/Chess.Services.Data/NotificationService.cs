namespace Chess.Services.Data
{
    using System;
    using System.Linq;
    using System.Text;

    using Chess.Common.Enums;
    using Chess.Services.Data.Contracts;
    using Chess.Services.Data.Models;
    using Chess.Services.Data.Models.EventArgs;
    using Chess.Services.Data.Models.Pieces;

    public class NotificationService : INotificationService
    {
        public event EventHandler OnMove;

        public event EventHandler OnHistoryUpdate;

        public void Invalid(bool oldCheck, Player movingPlayer)
        {
            if (movingPlayer.IsCheck && oldCheck)
            {
                this.OnMove?.Invoke(
                    movingPlayer,
                    new MoveArgs(Message.CheckSelf));
                return;
            }

            if (movingPlayer.IsCheck && !oldCheck)
            {
                this.OnMove?.Invoke(
                    movingPlayer,
                    new MoveArgs(Message.CheckOpen));

                movingPlayer.IsCheck = false;
                return;
            }

            this.OnMove?.Invoke(
                movingPlayer,
                new MoveArgs(Message.InvalidMove));
        }

        public void SendCheck(Player movingPlayer)
        {
            this.OnMove?.Invoke(
                movingPlayer,
                new MoveArgs(Message.CheckOpponent));
        }

        public void ClearCheck(Player movingPlayer, Player opponent)
        {
            if (!movingPlayer.IsCheck && !opponent.IsCheck)
            {
                this.OnMove?.Invoke(
                    movingPlayer,
                    new MoveArgs(Message.CheckClear));
            }
        }

        public void UpdateMoveHistory(Square source, Square target, Board board, Player movingPlayer, Player opponent, int turn, Move move)
        {
            string notation = this.GetAlgebraicNotation(source, target, board, opponent, turn, move);
            this.OnHistoryUpdate?.Invoke(movingPlayer, new HistoryUpdateArgs(notation));
        }

        private string GetAlgebraicNotation(Square source, Square target, Board board, Player opponent, int turn, Move move)
        {
            var sb = new StringBuilder();

            var playerTurn = Math.Ceiling(turn / 2.0);
            sb.Append(playerTurn + ". ");

            if (move.Type == MoveType.EnPassant)
            {
                var file = source.ToString()[0];
                sb.Append(file + "x" + target + "e.p");
            }
            else if (move.Type == MoveType.Castling)
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
            else if (move.Type == MoveType.PawnPromotion)
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

            if (opponent.IsCheck)
            {
                if (!opponent.IsCheckMate)
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
