<<<<<<< HEAD:Services/Chess.Services.Data/Models/Board.cs
﻿namespace Chess.Services.Data.Models
=======
﻿namespace Chess.Web.Models
>>>>>>> master:Web/Chess.Web/Models/Board.cs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Chess.Common;
    using Chess.Common.Enums;
    using Chess.Services.Data.Models.Pieces;
    using Chess.Services.Data.Models.Pieces.Contracts;

    public class Board : ICloneable
    {
        private readonly string[] files = new string[] { "a", "b", "c", "d", "e", "f", "g", "h" };

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

            for (int rank = 0; rank < Constants.Ranks; rank++)
            {
                for (int file = 0; file < Constants.Files; file++)
                {
                    var name = this.files[file] + (8 - rank);
                    var square = new Square()
                    {
                        Position = Factory.GetPosition(rank, file),
                        Piece = this.setup.FirstOrDefault(x => x.Key.Equals(name)).Value,
                        Color = toggle,
                        Name = name,
                    };

                    if (file != 7)
                    {
                        toggle = toggle == Color.White ? Color.Black : Color.White;
                    }

                    this.Matrix[rank][file] = square;
                }
            }
        }

        public void CalculateAttackedSquares()
        {
            for (int rank = 0; rank < Constants.Ranks; rank++)
            {
                for (int file = 0; file < Constants.Files; file++)
                {
                    this.Matrix[rank][file].IsAttacked.Clear();
                }
            }

            for (int rank = 0; rank < Constants.Ranks; rank++)
            {
                for (int file = 0; file < Constants.Files; file++)
                {
                    if (this.Matrix[rank][file].Piece != null)
                    {
                        this.Matrix[rank][file].Piece.Attacking(this.Matrix);
                    }
                }
            }
        }

        public void ShiftPiece(Move move)
        {
            move.Target.Piece = move.Source.Piece;
            move.Source.Piece = null;
        }

        public void ReversePiece(Move move, IPiece piece)
        {
            move.Source.Piece = move.Target.Piece;
            move.Target.Piece = piece;
        }

        public void ShiftEnPassant(Move move, int offsetX)
        {
            this.ShiftPiece(move);
            var square = this.GetSquareByCoordinates(move.Source.Position.Rank, move.Source.Position.File + offsetX);
            square.Piece = null;
        }

        public void ReverseEnPassant(Move move, int offsetX)
        {
            this.ReversePiece(move, null);
            var square = this.GetSquareByCoordinates(move.Source.Position.Rank, move.Source.Position.File + offsetX);
            var color = move.Source.Piece.Color == Color.White ? Color.Black : Color.White;
            square.Piece = Factory.GetPawn(color);
        }

        public Square GetKingSquare(Color color)
        {
            for (int rank = 0; rank < Constants.Ranks; rank++)
            {
                var square = this.Matrix[rank]
                    .FirstOrDefault(x =>
                        x.Piece is King &&
                        x.Piece.Color == color);

                if (square != null)
                {
                    return square;
                }
            }

            return null;
        }

        public Square GetSquareByCoordinates(int rank, int file)
        {
            return this.Matrix[rank][file];
        }

        public Square GetSquareByName(string name)
        {
            for (int rank = 0; rank < Constants.Ranks; rank++)
            {
                var square = this.Matrix[rank].FirstOrDefault(x => x.Name == name);

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

            for (int rank = 0; rank < Constants.Ranks; rank++)
            {
                for (int file = 0; file < Constants.Files; file++)
                {
                    board.Matrix[rank][file] = this.Matrix[rank][file].Clone() as Square;
                }
            }

            return board;
        }
    }
}
