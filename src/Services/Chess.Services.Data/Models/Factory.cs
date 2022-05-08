namespace Chess.Services.Data.Models
{
    using System;

    using Chess.Common.Constants;
    using Chess.Common.Enums;
    using Chess.Services.Data.Models.Pieces;
    using Chess.Services.Data.Models.Pieces.Contracts;
    using Chess.Services.Data.Models.Pieces.Helpers;
    using Chess.Services.Data.Services.Contracts;
    using Microsoft.Extensions.DependencyInjection;

    public class Factory
    {
        public static Game GetGame(
            Player player1,
            Player player2,
            IServiceProvider serviceProvider)
        {
            var notificationService = serviceProvider.GetRequiredService<INotificationService>();
            var checkService = serviceProvider.GetRequiredService<ICheckService>();
            var drawService = serviceProvider.GetRequiredService<IDrawService>();
            var utilityService = serviceProvider.GetRequiredService<IUtilityService>();

            return new (player1, player2, notificationService, checkService, drawService, utilityService, serviceProvider);
        }

        public static Player GetPlayer(string name, string connectionId, string userId) => new (name, connectionId, userId);

        public static Square[][] GetMatrix()
        {
            Square[][] matrix = new Square[BoardConstants.Ranks][];

            for (int rank = 0; rank < BoardConstants.Ranks; rank++)
            {
                matrix[rank] = new Square[BoardConstants.Files];
            }

            return matrix;
        }

        public static Move GetMove() => new ();

        public static Move GetMove(Square source, Square target) => new (source, target);

        public static Board GetBoard() => new ();

        public static Square GetSquare() => new ();

        public static Square GetSquare(int rank, int file) => new (rank, file);

        public static Position GetPosition() => new ();

        public static Position GetPosition(int rank, int file) => new (rank, file);

        public static IPiece GetPawn(Color color) => new Pawn(color);

        public static IPiece GetRook(Color color) => new Rook(color);

        public static IPiece GetKnight(Color color) => new Knight(color);

        public static IPiece GetBishop(Color color) => new Bishop(color);

        public static IPiece GetQueen(Color color) => new Queen(color);

        public static IPiece GetKing(Color color) => new King(color);

        public static RookBehaviour GetRookBehaviour() => new ();

        public static BishopBahaviour GetBishopBehaviour() => new ();

        public static CastlingArgs GetCastlingArgs() => new ();

        public static EnPassantArgs GetEnPassantArgs() => new ();

        public static PawnPromotionArgs GetPawnPromotionArgs() => new ();
    }
}
