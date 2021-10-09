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

        public string GetAlgebraicNotation(AlgebraicNotationDto model)
        {
            StringBuilder builder = new StringBuilder();

            var turnNotation = this.GetTurnNotation(model.Turn);
            var moveNotation = this.GetMoveNotation(model);
            var checkNotation = this.GetCheckNotation(model.Opponent);

            builder
                .Append(turnNotation);
            builder
                .Append(moveNotation);
            builder
                .Append(checkNotation);

            return builder.ToString();
        }

        private string GetTurnNotation(int turn)
        {
            return $"{Math.Ceiling(turn / 2.0)}. ";
        }

        private string GetMoveNotation(AlgebraicNotationDto model)
        {
            if (model.Move.Type == MoveType.EnPassant)
            {
                return this.GetEnPassantNotation(model.OldSource, model.OldTarget);
            }

            if (model.Move.Type == MoveType.Castling)
            {
                return this.GetCastlingNotation(model.OldTarget);
            }

            if (model.Move.Type == MoveType.PawnPromotion)
            {
                return this.GetPawnPromotionNotation(model.OldTarget);
            }

            if (model.OldSource.Piece is Pawn)
            {
                return this.GetPawnMoveNotation(model.OldSource, model.OldTarget);
            }

            return this.GetNormalNotation(model);
        }

        private string GetCheckNotation(Player opponent)
        {
            if (opponent.IsCheck)
            {
                return !opponent.IsCheckMate ? "+" : "#";
            }

            return string.Empty;
        }

        private string GetEnPassantNotation(Square oldSource, Square oldTarget)
        {
            var oldFile = oldSource.Name[0];

            return $"{oldFile}x{oldTarget}e.p";
        }

        private string GetCastlingNotation(Square oldTarget)
        {
            return oldTarget.Name[0] == 'g' ? "0-0" : "0-0-0";
        }

        private string GetPawnPromotionNotation(Square oldTarget)
        {
            return $"{oldTarget}=Q";
        }

        private string GetPawnMoveNotation(Square oldSource, Square oldTarget)
        {
            if (oldTarget.Piece == null)
            {
                return $"{oldTarget}";
            }

            var oldFile = oldSource.Name[0];

            return $"{oldFile}x{oldTarget}";
        }

        private string GetNormalNotation(AlgebraicNotationDto model)
        {
            var targetWithOldIsAttackedValue = model.OldBoard.Matrix
                .SelectMany(x => x)
                .Where(y => y.Name == model.OldTarget.Name)
                .FirstOrDefault();

            if (targetWithOldIsAttackedValue.IsAttacked
                .Count(piece =>
                    piece.Color == model.OldSource.Piece.Color &&
                    piece.Name.Contains(model.OldSource.Piece.Name)) > 1)
            {
                var secondPieceSquare = model.OldBoard.Matrix
                    .SelectMany(x => x)
                    .Where(square =>
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
                        return $"{model.OldSource.Piece.Symbol}{rank}{xCheck}{model.OldTarget}";
                    }

                    return $"{model.OldSource.Piece.Symbol}{rank}{xCheck}{model.OldTarget}";
                }

                var file = model.OldSource.Name[0];
                return $"{model.OldSource.Piece.Symbol}{file}{xCheck}{model.OldTarget}";
            }

            var check = model.OldTarget.Piece == null ? string.Empty : "x";
            return $"{model.OldSource.Piece.Symbol}{check}{model.OldTarget}";
        }
    }
}
