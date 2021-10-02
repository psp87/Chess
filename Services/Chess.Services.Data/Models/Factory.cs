namespace Chess.Services.Data.Models
{
    using System;

    using Chess.Common;
    using Chess.Common.Enums;
    using Chess.Services.Data.Contracts;
    using Chess.Services.Data.Models.Pieces;
    using Chess.Services.Data.Models.Pieces.Contracts;
    using Chess.Services.Data.Models.Pieces.Helpers;
    using Microsoft.Extensions.DependencyInjection;

    public class Factory
    {
        public static Game GetGame(Player player1, Player player2, IServiceProvider serviceProvider)
        {
            var drawService = serviceProvider.GetRequiredService<IDrawService>();
            var checkService = serviceProvider.GetRequiredService<ICheckService>();

            return new Game(player1, player2, drawService, checkService);
        }

        public static Player GetPlayer(string name, string connectionId, string userId) => new Player(name, connectionId, userId);

        public static Square[][] GetMatrix()
        {
            Square[][] matrix = new Square[Constants.Ranks][];

            for (int rank = 0; rank < Constants.Ranks; rank++)
            {
                matrix[rank] = new Square[Constants.Files];
            }

            return matrix;
        }

        public static Move GetMove() => new Move();

        public static Move GetMove(Square source, Square target) => new Move(source, target);

        public static Board GetBoard() => new Board();

        public static Square GetSquare() => new Square();

        public static Square GetSquare(int rank, int file) => new Square(rank, file);

        public static Position GetPosition() => new Position();

        public static Position GetPosition(int rank, int file) => new Position(rank, file);

        public static IPiece GetPawn(Color color) => new Pawn(color);

        public static IPiece GetRook(Color color) => new Rook(color);

        public static IPiece GetKnight(Color color) => new Knight(color);

        public static IPiece GetBishop(Color color) => new Bishop(color);

        public static IPiece GetQueen(Color color) => new Queen(color);

        public static IPiece GetKing(Color color) => new King(color);

        public static RookBehaviour GetRookBehaviour() => new RookBehaviour();

        public static BishopBahaviour GetBishopBehaviour() => new BishopBahaviour();

        public static CastlingArgs GetCastlingArgs() => new CastlingArgs();

        public static EnPassantArgs GetEnPassantArgs() => new EnPassantArgs();

        public static PawnPromotionArgs GetPawnPromotionArgs() => new PawnPromotionArgs();
    }
}
