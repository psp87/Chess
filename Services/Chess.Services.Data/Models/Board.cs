namespace Chess.Services.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Chess.Common.Constants;
    using Chess.Common.Enums;
    using Chess.Services.Data.Models.Pieces.Contracts;

    public class Board : ICloneable
    {
        public Board()
        {
            this.Initialize();
        }

        public Square[][] Matrix { get; set; } = Factory.GetMatrix();

        public static int GetSquaresBetween(
            Position attackPosition,
            Position kingPosition,
            AttackType attackType)
        {
            if (attackType == AttackType.Rank)
            {
                return Math.Abs(attackPosition.File - kingPosition.File) - 1;
            }

            return Math.Abs(attackPosition.Rank - kingPosition.Rank) - 1;
        }

        public static AttackType GetAttackType(Position attackPosition, Position kingPosition)
        {
            if (attackPosition.Rank == kingPosition.Rank)
            {
                return AttackType.Rank;
            }

            if (attackPosition.File == kingPosition.File)
            {
                return AttackType.File;
            }

            return AttackType.Diagonal;
        }

        public void ArrangePieces()
        {
            var setup = GetStandardSetup();

            foreach (var square in this.Matrix.SelectMany(x => x))
            {
                square.Piece = setup
                    .FirstOrDefault(x => x.Key
                        .Equals(square.Position.ToString()))
                    .Value;
            }
        }

        public void CalculateAttackedSquares()
        {
            foreach (var square in this.Matrix.SelectMany(x => x))
            {
                square.IsAttacked.Clear();
            }

            foreach (var square in this.Matrix.SelectMany(x => x))
            {
                if (square.Piece != null)
                {
                    square.Piece.Attacking(this.Matrix);
                }
            }
        }

        public void ShiftPiece(
            Square source,
            Square target,
            IPiece oldPiece = null)
        {
            target.Piece = source.Piece;
            source.Piece = oldPiece;

            this.CalculateAttackedSquares();
        }

        public void ShiftEnPassant(
            Square source,
            Square target,
            Square neighbourSquare,
            IPiece neighbourOldPiece = null)
        {
            target.Piece = source.Piece;
            source.Piece = null;
            neighbourSquare.Piece = neighbourOldPiece;

            this.CalculateAttackedSquares();
        }

        public Square GetKingSquare(Color color)
            => this.Matrix
                .SelectMany(x => x)
                .SingleOrDefault(x =>
                    x.Piece != null &&
                    x.Piece.IsType(SymbolConstants.King) &&
                    x.Piece.Color == color);

        public Square GetSquareByCoordinates(int rank, int file)
            => this.Matrix
                .SelectMany(x => x)
                .FirstOrDefault(x =>
                    x.Position.Rank == rank &&
                    x.Position.File == file);

        public Square GetSquareByName(string name)
            => this.Matrix
                .SelectMany(x => x)
                .FirstOrDefault(x => x.Name == name);

        public Square GetSecondPieceSquare(Square square)
            => this.Matrix
                .SelectMany(x => x)
                .Where(x =>
                    x.Piece != null &&
                    x.Piece.Color == square.Piece.Color &&
                    x.Piece.Name.Equals(square.Piece.Name) &&
                    x.Name != square.Name)
                .FirstOrDefault();

        public object Clone()
        {
            var board = Factory.GetBoard();

            for (int rank = BoardConstants.Rank8; rank <= BoardConstants.Rank1; rank++)
            {
                for (int file = BoardConstants.FileA; file <= BoardConstants.FileH; file++)
                {
                    board.Matrix[rank][file] = this.Matrix[rank][file].Clone() as Square;
                }
            }

            return board;
        }

        private static Dictionary<string, IPiece> GetStandardSetup()
            => new ()
            {
                { $"{BoardConstants.FileA},{BoardConstants.Rank1}", Factory.GetRook(Color.White) },
                { $"{BoardConstants.FileB},{BoardConstants.Rank1}", Factory.GetKnight(Color.White) },
                { $"{BoardConstants.FileC},{BoardConstants.Rank1}", Factory.GetBishop(Color.White) },
                { $"{BoardConstants.FileD},{BoardConstants.Rank1}", Factory.GetQueen(Color.White) },
                { $"{BoardConstants.FileE},{BoardConstants.Rank1}", Factory.GetKing(Color.White) },
                { $"{BoardConstants.FileF},{BoardConstants.Rank1}", Factory.GetBishop(Color.White) },
                { $"{BoardConstants.FileG},{BoardConstants.Rank1}", Factory.GetKnight(Color.White) },
                { $"{BoardConstants.FileH},{BoardConstants.Rank1}", Factory.GetRook(Color.White) },
                { $"{BoardConstants.FileA},{BoardConstants.Rank2}", Factory.GetPawn(Color.White) },
                { $"{BoardConstants.FileB},{BoardConstants.Rank2}", Factory.GetPawn(Color.White) },
                { $"{BoardConstants.FileC},{BoardConstants.Rank2}", Factory.GetPawn(Color.White) },
                { $"{BoardConstants.FileD},{BoardConstants.Rank2}", Factory.GetPawn(Color.White) },
                { $"{BoardConstants.FileE},{BoardConstants.Rank2}", Factory.GetPawn(Color.White) },
                { $"{BoardConstants.FileF},{BoardConstants.Rank2}", Factory.GetPawn(Color.White) },
                { $"{BoardConstants.FileG},{BoardConstants.Rank2}", Factory.GetPawn(Color.White) },
                { $"{BoardConstants.FileH},{BoardConstants.Rank2}", Factory.GetPawn(Color.White) },
                { $"{BoardConstants.FileA},{BoardConstants.Rank7}", Factory.GetPawn(Color.Black) },
                { $"{BoardConstants.FileB},{BoardConstants.Rank7}", Factory.GetPawn(Color.Black) },
                { $"{BoardConstants.FileC},{BoardConstants.Rank7}", Factory.GetPawn(Color.Black) },
                { $"{BoardConstants.FileD},{BoardConstants.Rank7}", Factory.GetPawn(Color.Black) },
                { $"{BoardConstants.FileE},{BoardConstants.Rank7}", Factory.GetPawn(Color.Black) },
                { $"{BoardConstants.FileF},{BoardConstants.Rank7}", Factory.GetPawn(Color.Black) },
                { $"{BoardConstants.FileG},{BoardConstants.Rank7}", Factory.GetPawn(Color.Black) },
                { $"{BoardConstants.FileH},{BoardConstants.Rank7}", Factory.GetPawn(Color.Black) },
                { $"{BoardConstants.FileA},{BoardConstants.Rank8}", Factory.GetRook(Color.Black) },
                { $"{BoardConstants.FileB},{BoardConstants.Rank8}", Factory.GetKnight(Color.Black) },
                { $"{BoardConstants.FileC},{BoardConstants.Rank8}", Factory.GetBishop(Color.Black) },
                { $"{BoardConstants.FileD},{BoardConstants.Rank8}", Factory.GetQueen(Color.Black) },
                { $"{BoardConstants.FileE},{BoardConstants.Rank8}", Factory.GetKing(Color.Black) },
                { $"{BoardConstants.FileF},{BoardConstants.Rank8}", Factory.GetBishop(Color.Black) },
                { $"{BoardConstants.FileG},{BoardConstants.Rank8}", Factory.GetKnight(Color.Black) },
                { $"{BoardConstants.FileH},{BoardConstants.Rank8}", Factory.GetRook(Color.Black) },
            };

        private void Initialize()
        {
            var toggle = Color.White;

            for (int rank = BoardConstants.Rank8; rank <= BoardConstants.Rank1; rank++)
            {
                for (int file = BoardConstants.FileA; file <= BoardConstants.FileH; file++)
                {
                    this.Matrix[rank][file] = new Square()
                    {
                        Position = Factory.GetPosition(rank, file),
                        Color = toggle,
                        Name = $"{(char)(file + 'a')}{8 - rank}",
                    };

                    if (file != 7)
                    {
                        toggle = toggle == Color.White ? Color.Black : Color.White;
                    }
                }
            }
        }
    }
}
