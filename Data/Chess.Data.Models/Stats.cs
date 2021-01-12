namespace Chess.Data.Models
{
    using Chess.Data.Common.Models;
    using Chess.Data.Models.Contracts;

    public class Stats : BaseModel<int>, IHaveOwner
    {
        public int Games { get; set; }

        public int Wins { get; set; }

        public int Draws { get; set; }

        public int Losses { get; set; }

        public int Rating { get; set; }

        public string OwnerId { get; set; }

        public virtual ApplicationUser Owner { get; set; }
    }
}
