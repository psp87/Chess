namespace Chess.Services.Data.Services.Contracts
{
    using Chess.Services.Data.Models;

    public interface ICheckService
    {
        bool IsCheck(Player player, Board board);

        bool IsCheckmate(Board board, Player movingPlayer, Player opponent, Game game);
    }
}
