namespace Chess.Data.Models.Contracts
{
    public interface IHaveOwner
    {
        string OwnerId { get; set; }

        ApplicationUser Owner { get; set; }
    }
}
