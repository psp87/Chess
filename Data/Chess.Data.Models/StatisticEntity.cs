namespace Chess.Data.Models
{
    using Chess.Data.Common.Models;
    using Chess.Data.Models.Contracts;

    public class StatisticEntity : BaseModel<int>, IHaveOwner
    {
        public int Played { get; set; }

        public int Won { get; set; }

        public int Drawn { get; set; }

        public int Lost { get; set; }

        public int EloRating { get; set; }

        public string UserId { get; set; }

        public virtual ChessUser User { get; set; }
    }
}
