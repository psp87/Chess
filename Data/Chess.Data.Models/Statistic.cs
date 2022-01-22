namespace Chess.Data.Models
{
    using Chess.Data.Common.Models;
    using Chess.Data.Models.Contracts;

    public class Statistic : BaseModel<int>, IHaveOwner
    {
        public int Games { get; set; }

        public int Win { get; set; }

        public int Draw { get; set; }

        public int Loss { get; set; }

        public int EloRating { get; set; }

        public string UserId { get; set; }

        public virtual ChessUser User { get; set; }
    }
}
