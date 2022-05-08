namespace Chess.Services.Data.Services
{
    using System.Linq;

    using Chess.Common.Constants;
    using Chess.Common.Enums;
    using Chess.Services.Data.Models;
    using Chess.Services.Data.Services.Contracts;

    public class CheckService : ICheckService
    {
        private Board board;
        private Player movingPlayer;
        private Player opponent;
        private Game game;

        public bool IsCheck(Player player, Board board)
        {
            var kingSquare = board
                .GetKingSquare(player.Color);

            if (kingSquare.IsAttacked
                .Where(x => x.Color != player.Color)
                .Any())
            {
                player.IsCheck = true;
                return true;
            }

            return false;
        }

        public bool IsCheckmate(Board board, Player movingPlayer, Player opponent, Game game)
        {
            this.board = board;
            this.movingPlayer = movingPlayer;
            this.opponent = opponent;
            this.game = game;

            if (!this.IsAbleToMoveKing() &&
                !this.IsAbleToTakeAttackingPiece() &&
                !this.IsAbleToBlockCheck())
            {
                opponent.IsCheckMate = true;
                return true;
            }

            return false;
        }

        private bool IsAbleToMoveKing()
        {
            var kingSquare = this.board
                .GetKingSquare(this.opponent.Color);

            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    if (offsetY == 0 && offsetX == 0)
                    {
                        continue;
                    }

                    var rank = kingSquare.Position.Rank + offsetY;
                    var file = kingSquare.Position.File + offsetX;

                    if (Position.IsInBoard(file, rank))
                    {
                        var offsetSquare = this.board
                            .GetSquareByCoordinates(rank, file);

                        if (this.IsSquareAvailable(offsetSquare))
                        {
                            var move = Factory
                                .GetMove(kingSquare, offsetSquare);

                            if (this.game.TryMove(this.opponent, move))
                            {
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
            var kingSquare = this.board
                .GetKingSquare(this.opponent.Color);
            var attackPiece = kingSquare.IsAttacked
                .Where(x => x.Color == this.movingPlayer.Color)
                .FirstOrDefault();
            var attackSquare = this.board
                .GetSquareByCoordinates(attackPiece.Position.Rank, attackPiece.Position.File);

            if (attackSquare.IsAttackedByColor(this.opponent.Color))
            {
                if ((attackSquare.IsAttacked.Count(x => x.Color == this.opponent.Color) > 1 ||
                    attackSquare.IsAttacked.Where(x => x.Color == this.opponent.Color).First().Symbol != SymbolConstants.King) &&
                    this.IsAnyPieceAbleToMoveWithoutOpenCheck(attackSquare))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAbleToBlockCheck()
        {
            var kingSquare = this.board
                .GetKingSquare(this.opponent.Color);
            var attackPiece = kingSquare.IsAttacked
                .Where(piece => piece.Color == this.movingPlayer.Color)
                .FirstOrDefault();

            if (!attackPiece.IsType(SymbolConstants.Knight, SymbolConstants.Pawn))
            {
                var attackType = Board.GetAttackType(attackPiece.Position, kingSquare.Position);

                return this.AbleToBlock(attackPiece.Position, kingSquare.Position, attackType);
            }

            return false;
        }

        private bool AbleToBlock(Position attack, Position king, AttackType attackType)
        {
            var squaresBetween = Board.GetSquaresBetween(attack, king, attackType);

            for (int i = 1; i <= squaresBetween; i++)
            {
                var offsetX = attackType == AttackType.File
                    ? 0
                    : attack.File - king.File < 0 ? i : -i;
                int offsetY = attackType == AttackType.Rank
                    ? 0
                    : attack.Rank - king.Rank < 0 ? i : -i;

                var offsetSquare = this.board
                    .GetSquareByCoordinates(attack.Rank + offsetY, attack.File + offsetX);

                var canBlock = attackType == AttackType.File
                    ? this.IsAbleToBlockWithAttackingPiece(offsetSquare)
                    : this.IsAbleToBlockWithAttackingPiece(offsetSquare) || this.IsAbleToBlockWithPawn(offsetSquare);

                if (canBlock)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsSquareAvailable(Square square)
        {
            if (square.Piece != null &&
                square.Piece.Color == this.movingPlayer.Color &&
               !square.IsAttackedByColor(this.movingPlayer.Color))
            {
                return true;
            }

            if (square.Piece == null &&
               !square.IsAttackedByColor(this.movingPlayer.Color))
            {
                return true;
            }

            return false;
        }

        private bool IsAbleToBlockWithPawn(Square current)
        {
            if ((this.opponent.Color == Color.White && current.Position.Rank < 6) ||
                (this.opponent.Color == Color.Black && current.Position.Rank > 1))
            {
                var offsetPlayer = this.opponent.Color == Color.White ? 1 : -1;
                var source = this.board
                    .GetSquareByCoordinates(current.Position.Rank + offsetPlayer, current.Position.File);

                if (this.IsOpponentPawn(source) &&
                    !this.DoesOpenCheck(source, current))
                {
                    return true;
                }

                if ((this.opponent.Color == Color.White && current.Position.Rank == 4) ||
                    (this.opponent.Color == Color.Black && current.Position.Rank == 3))
                {
                    source = this.board
                        .GetSquareByCoordinates(current.Position.Rank + (offsetPlayer * 2), current.Position.File);

                    if (this.IsOpponentPawn(source) &&
                        !this.DoesOpenCheck(source, current))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsAbleToBlockWithAttackingPiece(Square square)
        {
            if (square.IsAttackedByPiece(
                this.opponent.Color,
                SymbolConstants.Queen,
                SymbolConstants.Bishop,
                SymbolConstants.Knight,
                SymbolConstants.Rook))
            {
                if (this.IsAnyPieceAbleToMoveWithoutOpenCheck(square))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAnyPieceAbleToMoveWithoutOpenCheck(Square square)
        {
            var defendPieces = square.IsAttacked
                .Where(piece => piece.Color == this.opponent.Color)
                .ToArray();

            for (int i = 0; i < defendPieces.Length; i++)
            {
                var currentDefendPosition = defendPieces[i].Position;
                var currentDefendSquare = this.board.Matrix
                    .SelectMany(x => x)
                    .Where(y => y.Position
                    .Equals(currentDefendPosition))
                    .FirstOrDefault();

                var move = Factory
                    .GetMove(currentDefendSquare, square);

                if (this.game.TryMove(this.opponent, move))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsOpponentPawn(Square square)
        {
            if (square.Piece != null &&
                square.Piece.IsType(SymbolConstants.Pawn) &&
                square.Piece.Color == this.opponent.Color)
            {
                return true;
            }

            return false;
        }

        private bool DoesOpenCheck(Square source, Square target)
        {
            var move = Factory
                .GetMove(source, target);

            if (this.game.TryMove(this.opponent, move))
            {
                return false;
            }

            return true;
        }
    }
}
