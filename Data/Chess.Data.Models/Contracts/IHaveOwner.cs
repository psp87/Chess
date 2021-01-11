namespace Chess.Data.Models.Contracts
{
    public interface IHaveOwner
    {
        string ApplicationUserId { get; set; }

        ApplicationUser ApplicationUser { get; set; }
    }
}
