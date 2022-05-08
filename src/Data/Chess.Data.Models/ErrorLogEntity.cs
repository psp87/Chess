namespace Chess.Data.Models
{
    using Chess.Data.Common.Models;

    public class ErrorLogEntity : BaseModel<int>
    {
        public string GameId { get; set; }

        public virtual GameEntity Game { get; set; }

        public string Source { get; set; }

        public string Target { get; set; }

        public string FenString { get; set; }

        public string ExceptionMessage { get; set; }
    }
}
