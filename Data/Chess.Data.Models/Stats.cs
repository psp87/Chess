namespace Chess.Data.Models
{
    using Chess.Data.Common.Models;
    using Chess.Data.Models.Contracts;

    public class Stats : BaseModel<int>, IHaveOwner
    {
        public int Matches { get; set; }

        public int Wons { get; set; }

        public int Draws { get; set; }

        public int Losses { get; set; }

        public string ApplicationUserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
