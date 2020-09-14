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
        private readonly string[] letters = new string[] { "a", "b", "c", "d", "e", "f", "g", "h" };

        private readonly Dictionary<string, IPiece> setup = new Dictionary<string, IPiece>()
        {
            { "a1", Factory.GetRook(Color.White) }, { "b1", Factory.GetKnight(Color.White) }, { "c1", Factory.GetBishop(Color.White) }, { "d1", Factory.GetQueen(Color.White) },
            { "e1", Factory.GetKing(Color.White) }, { "f1", Factory.GetBishop(Color.White) }, { "g1", Factory.GetKnight(Color.White) }, { "h1", Factory.GetRook(Color.White) },
            { "a2", Factory.GetPawn(Color.White) }, { "b2", Factory.GetPawn(Color.White) }, { "c2", Factory.GetPawn(Color.White) }, { "d2", Factory.GetPawn(Color.White) },
            { "e2", Factory.GetPawn(Color.White) }, { "f2", Factory.GetPawn(Color.White) }, { "g2", Factory.GetPawn(Color.White) }, { "h2", Factory.GetPawn(Color.White) },
            { "a7", Factory.GetPawn(Color.Black) }, { "b7", Factory.GetPawn(Color.Black) }, { "c7", Factory.GetPawn(Color.Black) }, { "d7", Factory.GetPawn(Color.Black) },
            { "e7", Factory.GetPawn(Color.Black) }, { "f7", Factory.GetPawn(Color.Black) }, { "g7", Factory.GetPawn(Color.Black) }, { "h7", Factory.GetPawn(Color.Black) },
            { "a8", Factory.GetRook(Color.Black) }, { "b8", Factory.GetKnight(Color.Black) }, { "c8", Factory.GetBishop(Color.Black) }, { "d8", Factory.GetQueen(Color.Black) },
            { "e8", Factory.GetKing(Color.Black) }, { "f8", Factory.GetBishop(Color.Black) }, { "g8", Factory.GetKnight(Color.Black) }, { "h8", Factory.GetRook(Color.Black) },
        };

        public Board()
        {
            this.Matrix = Factory.GetMatrix();
            this.Initialize();
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

        public void ShiftPiece(Move move)
        {
            move.Target.Piece = move.Source.Piece;
            move.Source.Piece = null;
        }

        public void ReversePiece(Move move)
        {
            move.Source.Piece = move.Target.Piece;
            move.Target.Piece = null;
        }

        public void ShiftEnPassant(Move move, int x)
        {
            this.ShiftPiece(move);
            var square = this.GetSquareByCoordinates(move.Source.Position.Y, move.Source.Position.X + x);
            square.Piece = null;
        }

        public void ReverseEnPassant(Move move, int x)
        {
            this.ReversePiece(move);
            var square = this.GetSquareByCoordinates(move.Source.Position.Y, move.Source.Position.X + x);
            var color = move.Source.Piece.Color == Color.White ? Color.Black : Color.White;
            square.Piece = Factory.GetPawn(color);
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

        public Square GetSquareByCoordinates(int y, int x)
        {
            return this.Matrix[y][x];
        }

        public Square GetSquareByName(string name)
        {
            for (int y = 0; y < GlobalConstants.BoardRows; y++)
            {
                var square = this.Matrix[y].FirstOrDefault(x => x.Name == name);

                if (square != null)
                {
                    return square;
                }
            }

            return null;
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
    }
}
