namespace Chess.Services.Data.Services.Contracts
{
    using Chess.Services.Data.Dtos;
    using Chess.Services.Data.Models;

    public interface IUtilityService
    {
        string GetAlgebraicNotation(AlgebraicNotationDto model);

        void GetPawnPromotionFenString(string targetFen, Player movingPlayer, Move move);

        int CalculateRatingPoints(int yourRating, int opponentRating);
    }
}
