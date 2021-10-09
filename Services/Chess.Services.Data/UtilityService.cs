namespace Chess.Services.Data
{
    using System;
    using System.Linq;
    using System.Text;

    using Chess.Services.Data.Contracts;
    using Chess.Services.Data.Dtos;
    using Chess.Services.Data.Models;
    using Chess.Services.Data.Models.Pieces;
    using Common.Enums;

    public class UtilityService : IUtilityService
    {
        public string GetAlgebraicNotation(AlgebraicNotationDto model)
        {
            var sb = new StringBuilder();

            var playerTurn = Math.Ceiling(model.Turn / 2.0);
            sb.Append(playerTurn + ". ");

            if (model.Move.Type == MoveType.EnPassant)
            {
                var file = model.OldSource.ToString()[0];
                sb.Append(file + "x" + model.OldTarget + "e.p");
            }
            else if (model.Move.Type == MoveType.Castling)
            {
                if (model.OldTarget.ToString()[0] == 'g')
                {
                    sb.Append("0-0");
                }
                else
                {
                    sb.Append("0-0-0");
                }
            }
            else if (model.Move.Type == MoveType.PawnPromotion)
            {
                sb.Append(model.OldTarget + "=Q");
            }
            else if (model.OldTarget.Piece == null || model.OldTarget.Piece.Color != model.OldSource.Piece.Color)
            {
                if (model.OldSource.Piece is Pawn)
                {
                    if (model.OldTarget.Piece == null)
                    {
                        sb.Append(model.OldTarget);
                    }
                    else
                    {
                        var file = model.OldSource.ToString()[0];
                        sb.Append(file + "x" + model.OldTarget);
                    }
                }
                else
                {
                    var targetWithOldIsAttackedValue = model.OldBoard.Matrix.SelectMany(x => x).Where(y => y.Name == model.OldTarget.Name).FirstOrDefault();

                    if (targetWithOldIsAttackedValue.IsAttacked.Count(piece =>
                        piece.Color == model.OldSource.Piece.Color &&
                        piece.Name.Contains(model.OldSource.Piece.Name)) > 1)
                    {
                        var secondPieceSquare = model.OldBoard.Matrix.SelectMany(x => x).Where(square =>
                            square.Piece != null &&
                            square.Piece.Color == model.OldSource.Piece.Color &&
                            square.Piece.Name.Contains(model.OldSource.Piece.Name) &&
                            square.Name != model.OldSource.Name).FirstOrDefault();

                        var xCheck = model.OldTarget.Piece == null ? string.Empty : "x";

                        if (secondPieceSquare.Position.File.Equals(model.OldSource.Position.File))
                        {
                            var rank = model.OldSource.Name[1];
                            if (model.OldTarget.Piece == null)
                            {
                                sb.Append($"{model.OldSource.Piece.Symbol}{rank}{xCheck}{model.OldTarget}");
                            }
                            else
                            {
                                sb.Append($"{model.OldSource.Piece.Symbol}{rank}{xCheck}{model.OldTarget}");
                            }
                        }
                        else
                        {
                            var file = model.OldSource.Name[0];
                            sb.Append($"{model.OldSource.Piece.Symbol}{file}{xCheck}{model.OldTarget}");
                        }
                    }
                    else
                    {
                        var check = model.OldTarget.Piece == null ? string.Empty : "x";
                        sb.Append($"{model.OldSource.Piece.Symbol}{check}{model.OldTarget}");
                    }
                }
            }

            if (model.Opponent.IsCheck)
            {
                if (!model.Opponent.IsCheckMate)
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

        public int CalculateRatingPoints(int yourRating, int opponentRating)
        {
            return Math.Max(1, ((opponentRating - yourRating) / 25) + 16);
        }

        public void GetPawnPromotionFenString(string targetFen, Player movingPlayer, Move move)
        {
            var sb = new StringBuilder();
            string[] rows = targetFen.Split('/');

            var lastRow = movingPlayer.Color == Color.White ? 0 : 7;
            var pawn = movingPlayer.Color == Color.White ? "P" : "p";
            var queen = movingPlayer.Color == Color.White ? "Q" : "q";

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
                    if (movingPlayer.Color == Color.White)
                    {
                        sb.Append("/" + currentRow);
                    }
                    else
                    {
                        sb.Append(currentRow + "/");
                    }
                }
            }

            move.PawnPromotionArgs.FenString = sb.ToString();
        }
    }
}
