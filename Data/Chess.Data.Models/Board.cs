namespace Chess.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Chess.Common;
    using Chess.Common.Enums;
    using Chess.Data.Models.Pieces;
    using Chess.Data.Models.Pieces.Contracts;
    using Chess.Data.Models.Pieces.Helpers;

    public class Board : ICloneable
    {
        private Queue<Move> movesQueue;

        private string[] letters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H" };

        private Dictionary<string, Piece> setup = new Dictionary<string, Piece>()
        {
            { "A1", new Rook(Color.Light) },  { "B1", new Knight(Color.Light) }, { "C1", new Bishop(Color.Light) }, { "D1", new Queen(Color.Light) },
            { "E1", new King(Color.Light) }, { "F1", new Bishop(Color.Light) }, { "G1", new Knight(Color.Light) }, { "H1", new Rook(Color.Light) },
            { "A2", new Pawn(Color.Light) },  { "B2", new Pawn(Color.Light) },   { "C2", new Pawn(Color.Light) },   { "D2", new Pawn(Color.Light) },
            { "E2", new Pawn(Color.Light) },  { "F2", new Pawn(Color.Light) },   { "G2", new Pawn(Color.Light) },   { "H2", new Pawn(Color.Light) },

            { "A7", new Pawn(Color.Dark) },  { "B7", new Pawn(Color.Dark) },   { "C7", new Pawn(Color.Dark) },   { "D7", new Pawn(Color.Dark) },
            { "E7", new Pawn(Color.Dark) },  { "F7", new Pawn(Color.Dark) },   { "G7", new Pawn(Color.Dark) },   { "H7", new Pawn(Color.Dark) },
            { "A8", new Rook(Color.Dark) },  { "B8", new Knight(Color.Dark) }, { "C8", new Bishop(Color.Dark) }, { "D8", new Queen(Color.Dark) },
            { "E8", new King(Color.Dark) }, { "F8", new Bishop(Color.Dark) }, { "G8", new Knight(Color.Dark) }, { "H8", new Rook(Color.Dark) },
        };

        public Board()
        {
            this.movesQueue = new Queue<Move>();

            this.Matrix = Factory.GetMatrix();
            this.Move = Factory.GetMove();
        }

        public Square[][] Matrix { get; set; }

        public Move Move { get; set; }

        public bool MakeMove(string source, string target, Player movingPlayer, Player opponent)
        {
            this.Move.Start = this.GetSquare(source);
            this.Move.End = this.GetSquare(target);

            if (this.MovePiece(movingPlayer, opponent) ||
                this.TakePiece(movingPlayer, opponent) ||
                this.EnPassantTake(movingPlayer, opponent))
            {
                if (movingPlayer.IsCheck)
                {
                    return false;
                }

                if (this.IsPlayerChecked(opponent))
                {
                    this.IsOpponentCheckmate(movingPlayer, opponent, this.Move.End);
                }

                this.IsGameRepetitionDraw();
                this.IsGameDraw();
                this.IsGameStalemate(opponent);

                return true;
            }

            return false;
        }

        public void Initialize()
        {
            var toggle = Color.Light;

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

                    if (square.Piece == null)
                    {
                        square.Piece = Factory.GetEmpty();
                        square.IsOccupied = false;
                    }

                    if (col != 7)
                    {
                        toggle = toggle == Color.Light ? Color.Dark : Color.Light;
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

        private bool MovePiece(Player movingPlayer, Player opponent)
        {
            if (!this.Move.End.IsOccupied &&
                movingPlayer.Color == this.Move.Start.Piece.Color &&
                this.Move.Start.Piece.Move(this.Move.End.Position, this.Matrix))
            {
                if (!this.TryMove(movingPlayer, opponent))
                {
                    movingPlayer.IsCheck = true;
                }

                return true;
            }

            return false;
        }

        private bool TakePiece(Player movingPlayer, Player opponent)
        {
            if (this.Move.End.IsOccupied &&
                this.Move.End.Piece.Color != this.Move.Start.Piece.Color &&
                movingPlayer.Color == this.Move.Start.Piece.Color &&
                this.Move.Start.Piece.Take(this.Move.End.Position, this.Matrix))
            {
                string pieceName = this.Move.End.Piece.Name;

                if (!this.TryMove(movingPlayer, opponent))
                {
                    movingPlayer.IsCheck = true;
                    return true;
                }

                movingPlayer.TakeFigure(pieceName);
                return true;
            }

            return false;
        }

        private bool EnPassantTake(Player movingPlayer, Player opponent)
        {
            if (EnPassant.Turn == GlobalConstants.TurnCounter &&
                this.Move.End.Position.X == EnPassant.PositionX &&
                this.Move.End.Position.Y == EnPassant.PositionY &&
                this.Move.Start.Piece is Pawn)
            {
                this.PlacePiece(this.Move);
                this.RemovePiece(this.Move.Start);
                int x = this.Move.End.Position.X > this.Move.Start.Position.X ? 1 : -1;
                IPiece piece = this.Matrix[this.Move.Start.Position.Y][this.Move.Start.Position.X + x].Piece;
                this.RemovePiece(this.Matrix[this.Move.Start.Position.Y][this.Move.Start.Position.X + x]);
                this.CalculateAttackedSquares();

                if (this.IsPlayerChecked(movingPlayer))
                {
                    this.ReversePiece(this.Move);
                    this.RemovePiece(this.Move.End);
                    this.Matrix[this.Move.Start.Position.Y][this.Move.Start.Position.X + x].Piece = piece;
                    this.CalculateAttackedSquares();
                    movingPlayer.IsCheck = true;
                    return true;
                }

                movingPlayer.IsCheck = false;

                string position = this.GetStringPosition(this.Move.Start.Position.X + x, this.Move.Start.Position.Y);
                GlobalConstants.EnPassantTake = position;

                return true;
            }

            return false;
        }

        private string GetStringPosition(int x, int y)
        {
            char col = (char)(97 + x);
            int row = Math.Abs(y - 8);
            return $"{col}{row}";
        }

        private bool TryMove(Player movingPlayer, Player opponent)
        {
            this.PlacePiece(this.Move);
            this.RemovePiece(this.Move.Start);
            this.CalculateAttackedSquares();

            if (this.IsPlayerChecked(movingPlayer))
            {
                this.ReversePiece(this.Move);
                this.RemovePiece(this.Move.End);
                this.CalculateAttackedSquares();
                return false;
            }

            if (this.Move.End.Piece is Pawn && this.Move.End.Piece.IsLastMove)
            {
                // this.Move.End.Piece = this.drawer.PawnPromotion(this.Move.End);
                this.CalculateAttackedSquares();
            }

            movingPlayer.IsCheck = false;
            return true;
        }

        private void PlacePiece(Move move)
        {
            this.Matrix[move.End.Position.Y][move.End.Position.X].Piece = this.Matrix[move.Start.Position.Y][move.Start.Position.X].Piece;
        }

        private void RemovePiece(Square square)
        {
            IPiece empty = Factory.GetEmpty();

            this.Matrix[square.Position.Y][square.Position.X].Piece = empty;
        }

        private void ReversePiece(Move move)
        {
            this.Matrix[move.Start.Position.Y][move.Start.Position.X].Piece = this.Matrix[move.End.Position.Y][move.End.Position.X].Piece;
        }

        private bool IsPlayerChecked(Player player)
        {
            var kingSquare = this.GetKingSquare(player.Color);

            if (kingSquare.IsAttacked.Where(x => x.Color != kingSquare.Piece.Color).Any())
            {
                player.IsCheck = true;
                return true;
            }

            return false;
        }

        private void IsOpponentCheckmate(Player movingPlayer, Player opponent, Square attackingSquare)
        {
            var king = this.GetKingSquare(opponent.Color);

            if (!this.IsKingAbleToMove(king, movingPlayer) &&
                !this.AttackingPieceCanBeTaken(attackingSquare, opponent) &&
                !this.OtherPieceCanBlockTheCheck(king, attackingSquare, opponent))
            {
                GlobalConstants.GameOver = GameOver.Checkmate;
            }
        }

        private void IsGameStalemate(Player player)
        {
            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                for (int x = 0; x < GlobalConstants.BoardCols; x++)
                {
                    var currentFigure = this.Matrix[y][x].Piece;

                    if (currentFigure.Color == player.Color)
                    {
                        currentFigure.IsMoveAvailable(this.Matrix);
                        if (currentFigure.IsMovable)
                        {
                            return;
                        }
                    }
                }
            }

            GlobalConstants.GameOver = GameOver.Stalemate;
        }

        private void IsGameRepetitionDraw()
        {
            var move = Factory.GetMove(this.Move);
            this.movesQueue.Enqueue(move);

            Move[] array = this.movesQueue.ToArray();

            if (this.movesQueue.Count == 3 &&
                array[0].Start.Position.X == this.Move.End.Position.X &&
                array[0].Start.Position.Y == this.Move.End.Position.Y &&
                array[0].End.Position.X == this.Move.Start.Position.X &&
                array[0].End.Position.Y == this.Move.Start.Position.Y)
            {
                return;
            }
            else if (this.movesQueue.Count == 4 &&
                array[1].Start.Position.X == this.Move.End.Position.X &&
                array[1].Start.Position.Y == this.Move.End.Position.Y &&
                array[1].End.Position.X == this.Move.Start.Position.X &&
                array[1].End.Position.Y == this.Move.Start.Position.Y)
            {
                return;
            }
            else if (this.movesQueue.Count == 5 &&
                array[0].Start.Position.X == this.Move.Start.Position.X &&
                array[0].Start.Position.Y == this.Move.Start.Position.Y &&
                array[0].End.Position.X == this.Move.End.Position.X &&
                array[0].End.Position.Y == this.Move.End.Position.Y)
            {
                return;
            }
            else if (this.movesQueue.Count == 6 &&
                array[1].Start.Position.X == this.Move.Start.Position.X &&
                array[1].Start.Position.Y == this.Move.Start.Position.Y &&
                array[1].End.Position.X == this.Move.End.Position.X &&
                array[1].End.Position.Y == this.Move.End.Position.Y)
            {
                GlobalConstants.GameOver = GameOver.Repetition;
                return;
            }
            else if (this.movesQueue.Count > 2)
            {
                this.movesQueue.Dequeue();
            }
        }

        private void IsGameDraw()
        {
            int counterBishopKnightWhite = 0;
            int counterBishopKnightBlack = 0;

            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                for (int x = 0; x < GlobalConstants.BoardCols; x++)
                {
                    var currentFigure = this.Matrix[y][x].Piece;

                    if (!(currentFigure is Empty || currentFigure is King))
                    {
                        if (currentFigure is Pawn ||
                            currentFigure is Rook ||
                            currentFigure is Queen ||
                            counterBishopKnightWhite > 1 ||
                            counterBishopKnightBlack > 1)
                        {
                            return;
                        }

                        if (currentFigure.Color == Color.Light)
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

            GlobalConstants.GameOver = GameOver.Draw;
        }

        private void CalculateAttackedSquares()
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
                    if (this.Matrix[y][x].IsOccupied == true)
                    {
                        this.Matrix[y][x].Piece.Attacking(this.Matrix);
                    }
                }
            }
        }

        private Square GetSquare(string position)
        {
            int col = char.Parse(position[0].ToString().ToUpper()) - 65;
            int row = Math.Abs(int.Parse(position[1].ToString()) - 8);

            return this.Matrix[row][col];
        }

        private Square GetKingSquare(Color color)
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

        #region IsOpponentCheckmate Methods
        private bool IsKingAbleToMove(Square king, Player movingPlayer)
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
                            var empty = Factory.GetEmpty();
                            var neighbourFigure = this.Matrix[kingY + y][kingX + x].Piece;

                            this.AssignNewValuesAndCalculate(kingY, kingX, y, x, currentFigure, empty);

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

        private bool AttackingPieceCanBeTaken(Square attackingSquare, Player opponent)
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

        private bool OtherPieceCanBlockTheCheck(Square king, Square attackingSquare, Player opponent)
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
                        var signPlayer = opponent.Color == Color.Light ? 1 : -1;

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
                        var signPlayer = opponent.Color == Color.Light ? 1 : -1;

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
            if (square.IsOccupied &&
                square.Piece.Color == movingPlayer.Color &&
                !square.IsAttacked.Where(x => x.Color == movingPlayer.Color).Any())
            {
                return true;
            }

            if (!square.IsOccupied &&
                !square.IsAttacked.Where(x => x.Color == movingPlayer.Color).Any())
            {
                return true;
            }

            return false;
        }

        private void AssignNewValuesAndCalculate(int kingRow, int kingCol, int i, int k, IPiece currentFigure, IPiece empty)
        {
            this.Matrix[kingRow][kingCol].Piece = empty;
            this.Matrix[kingRow][kingCol].IsOccupied = false;
            this.Matrix[kingRow + i][kingCol + k].Piece = currentFigure;
            this.Matrix[kingRow + i][kingCol + k].IsOccupied = true;
            this.CalculateAttackedSquares();
        }

        private void AssignOldValuesAndCalculate(int kingRow, int kingCol, int i, int k, IPiece currentFigure, IPiece neighbourFigure)
        {
            this.Matrix[kingRow][kingCol].Piece = currentFigure;
            this.Matrix[kingRow][kingCol].IsOccupied = true;
            this.Matrix[kingRow + i][kingCol + k].Piece = neighbourFigure;
            this.Matrix[kingRow + i][kingCol + k].IsOccupied = true;
            if (neighbourFigure is Empty)
            {
                this.Matrix[kingRow + i][kingCol + k].IsOccupied = false;
            }

            this.CalculateAttackedSquares();
        }
        #endregion
    }
}
