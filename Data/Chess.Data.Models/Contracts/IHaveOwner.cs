namespace Chess.Data.Models.Contracts
{
    public interface IHaveOwner
    {
        string UserId { get; set; }

        ChessUser User { get; set; }
    }
}
