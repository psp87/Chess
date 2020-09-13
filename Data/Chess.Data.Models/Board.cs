namespace Chess.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Chess.Common;
    using Chess.Common.Enums;
    using Chess.Data.Models.Pieces;
    using Chess.Data.Models.Pieces.Contracts;

    public class Board : ICloneable
    {
        private string[] letters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H" };

        private Dictionary<string, Piece> setup = new Dictionary<string, Piece>()
        {
            { "A1", new Rook(Color.White) },  { "B1", new Knight(Color.White) }, { "C1", new Bishop(Color.White) }, { "D1", new Queen(Color.White) },
            { "E1", new King(Color.White) }, { "F1", new Bishop(Color.White) }, { "G1", new Knight(Color.White) }, { "H1", new Rook(Color.White) },
            { "A2", new Pawn(Color.White) },  { "B2", new Pawn(Color.White) },   { "C2", new Pawn(Color.White) },   { "D2", new Pawn(Color.White) },
            { "E2", new Pawn(Color.White) },  { "F2", new Pawn(Color.White) },   { "G2", new Pawn(Color.White) },   { "H2", new Pawn(Color.White) },
            { "A7", new Pawn(Color.Black) },  { "B7", new Pawn(Color.Black) },   { "C7", new Pawn(Color.Black) },   { "D7", new Pawn(Color.Black) },
            { "E7", new Pawn(Color.Black) },  { "F7", new Pawn(Color.Black) },   { "G7", new Pawn(Color.Black) },   { "H7", new Pawn(Color.Black) },
            { "A8", new Rook(Color.Black) },  { "B8", new Knight(Color.Black) }, { "C8", new Bishop(Color.Black) }, { "D8", new Queen(Color.Black) },
            { "E8", new King(Color.Black) }, { "F8", new Bishop(Color.Black) }, { "G8", new Knight(Color.Black) }, { "H8", new Rook(Color.Black) },
        };

        public Board()
        {
            this.Matrix = Factory.GetMatrix();
        }

        public Square[][] Matrix { get; set; }

        public void Initialize()
        {
            var toggle = Color.White;

            for (int row = 0; row < GlobalConstants.BoardRows; row++)
            {
                for (int col = 0; col < GlobalConstants.BoardCols; col++)
                {
                    var name = this.letters[col] + (8 - row);
                    var square = new Square()
                    {
                        Position = Factory.GetPosition(row, col),
                        Piece = this.setup.FirstOrDefault(x => x.Key.Equals(name)).Value,
                        Color = toggle,
                        Name = name,
                    };

                    if (col != 7)
                    {
                        toggle = toggle == Color.White ? Color.Black : Color.White;
                    }

                    this.Matrix[row][col] = square;
                }
            }
        }

        public object Clone()
        {
            var board = Factory.GetBoard();

            for (int row = 0; row <= 7; row++)
            {
                for (int col = 0; col <= 7; col++)
                {
                    board.Matrix[row][col] = this.Matrix[row][col].Clone() as Square;
                }
            }

            return board;
        }

        public bool IsPlayerChecked(Player player)
        {
            var kingSquare = this.GetKingSquare(player.Color);

            if (kingSquare.IsAttacked.Where(x => x.Color != kingSquare.Piece.Color).Any())
            {
                player.IsCheck = true;
                return true;
            }

            return false;
        }

        public void ShiftPiece(Square source, Square target)
        {
            this.PlacePiece(source, target);
            this.RemovePiece(source);
        }

        public void Reverse(Square source, Square target)
        {
            this.ReversePiece(source, target);
            this.RemovePiece(target);
        }

        public void CalculateAttackedSquares()
        {
            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                for (int x = 0; x < GlobalConstants.BoardCols; x++)
                {
                    this.Matrix[y][x].IsAttacked.Clear();
                }
            }

            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                for (int x = 0; x < GlobalConstants.BoardCols; x++)
                {
                    if (this.Matrix[y][x].Piece != null)
                    {
                        this.Matrix[y][x].Piece.Attacking(this.Matrix);
                    }
                }
            }
        }

        public Square GetSquare(string position)
        {
            int col = char.Parse(position[0].ToString().ToUpper()) - 65;
            int row = Math.Abs(int.Parse(position[1].ToString()) - 8);

            return this.Matrix[row][col];
        }

        public Square GetSquareByCoordinates(int y, int x)
        {
            return this.Matrix[y][x];
        }

        public Square GetKingSquare(Color color)
        {
            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                var kingSquare = this.Matrix[y].FirstOrDefault(x => x.Piece is King && x.Piece.Color == color);

                if (kingSquare != null)
                {
                    return kingSquare;
                }
            }

            return null;
        }

        public void ShiftEnPassant(int x, Move move)
        {
            this.PlacePiece(move.Source, move.Target);
            this.RemovePiece(move.Source);
            this.RemovePiece(this.Matrix[move.Source.Position.Y][move.Source.Position.X + x]);
        }

        public void ReverseEnPassant(int x, Move move)
        {
            this.ReversePiece(move.Source, move.Target);
            this.RemovePiece(move.Target);
            var color = this.Matrix[move.Source.Position.Y][move.Source.Position.X + x].Piece.Color == Color.White ? Color.White : Color.Black;
            this.Matrix[move.Source.Position.Y][move.Source.Position.X + x].Piece = Factory.GetPawn(color);
        }

        public bool IsKingAbleToMove(Square king, Player movingPlayer)
        {
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
                        var checkedSquare = this.Matrix[kingY + y][kingX + x];

                        if (this.NeighbourSquareAvailable(checkedSquare, movingPlayer))
                        {
                            var currentFigure = this.Matrix[kingY][kingX].Piece;
                            var neighbourFigure = this.Matrix[kingY + y][kingX + x].Piece;

                            this.AssignNewValuesAndCalculate(kingY, kingX, y, x, currentFigure);

                            if (!this.Matrix[kingY + y][kingX + x].IsAttacked.Where(k => k.Color == movingPlayer.Color).Any())
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

        public bool AttackingPieceCanBeTaken(Square attackingSquare, Player opponent)
        {
            if (attackingSquare.IsAttacked.Where(x => x.Color == opponent.Color).Any())
            {
                if (attackingSquare.IsAttacked.Count(x => x.Color == opponent.Color) > 1)
                {
                    return true;
                }
                else if (!(attackingSquare.IsAttacked.Where(x => x.Color == opponent.Color).First() is King))
                {
                    return true;
                }
            }

            return false;
        }

        public bool OtherPieceCanBlockTheCheck(Square king, Square attackingSquare, Player opponent)
        {
            if (!(attackingSquare.Piece is Knight) && !(attackingSquare.Piece is Pawn))
            {
                int kingY = king.Position.Y;
                int kingX = king.Position.X;

                int attackingRow = attackingSquare.Position.Y;
                int attackingCol = attackingSquare.Position.X;

                if (attackingRow == kingY)
                {
                    int difference = Math.Abs(attackingCol - kingX) - 1;

                    for (int i = 1; i <= difference; i++)
                    {
                        int sign = attackingCol - kingX < 0 ? i : -i;
                        var signPlayer = opponent.Color == Color.White ? 1 : -1;

                        var currentSquare = this.Matrix[kingY][attackingCol + sign];
                        var neighbourSquare = this.Matrix[kingY][attackingCol + sign + signPlayer];

                        if (currentSquare.IsAttacked.Where(x => x.Color == opponent.Color && !(x is King) && !(x is Pawn)).Any() ||
                            (neighbourSquare.Piece is Pawn && neighbourSquare.Piece.Color == opponent.Color))
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

                        if (this.Matrix[attackingRow + sign][kingX].IsAttacked.Where(x => x.Color == opponent.Color && !(x is King) && !(x is Pawn)).Any())
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
                        var signPlayer = opponent.Color == Color.White ? 1 : -1;

                        var currentSquare = this.Matrix[attackingRow + signRow][attackingCol + signCol];
                        var neighbourSquare = this.Matrix[attackingRow + signRow + signPlayer][attackingCol + signCol];

                        if (currentSquare.IsAttacked.Where(x => x.Color == opponent.Color && !(x is King) && !(x is Pawn)).Any() ||
                            (neighbourSquare.Piece is Pawn && neighbourSquare.Piece.Color == opponent.Color))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool NeighbourSquareAvailable(Square square, Player movingPlayer)
        {
            if (square.Piece != null &&
                square.Piece.Color == movingPlayer.Color &&
                !square.IsAttacked.Where(x => x.Color == movingPlayer.Color).Any())
            {
                return true;
            }

            if (square.Piece == null &&
                !square.IsAttacked.Where(x => x.Color == movingPlayer.Color).Any())
            {
                return true;
            }

            return false;
        }

        private void AssignNewValuesAndCalculate(int kingRow, int kingCol, int i, int k, IPiece currentFigure)
        {
            this.Matrix[kingRow][kingCol].Piece = null;
            this.Matrix[kingRow + i][kingCol + k].Piece = currentFigure;
            this.CalculateAttackedSquares();
        }

        private void AssignOldValuesAndCalculate(int kingRow, int kingCol, int i, int k, IPiece currentFigure, IPiece neighbourFigure)
        {
            this.Matrix[kingRow][kingCol].Piece = currentFigure;
            this.Matrix[kingRow + i][kingCol + k].Piece = neighbourFigure;

            this.CalculateAttackedSquares();
        }

        #region Board Base Methods
        private void PlacePiece(Square source, Square target)
        {
            this.Matrix[target.Position.Y][target.Position.X].Piece = this.Matrix[source.Position.Y][source.Position.X].Piece;
        }

        private void RemovePiece(Square square)
        {
            this.Matrix[square.Position.Y][square.Position.X].Piece = null;
        }

        private void ReversePiece(Square source, Square target)
        {
            this.Matrix[source.Position.Y][source.Position.X].Piece = this.Matrix[target.Position.Y][target.Position.X].Piece;
        }
        #endregion
    }
}
