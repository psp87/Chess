namespace Chess.Data.Models
{
    using System;

    public class ErrorLog
    {
        public int Id { get; set; }

        public string GameId { get; set; }

        public virtual Game Game { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Source { get; set; }

        public string Target { get; set; }

        public string FenString { get; set; }

        public string ExceptionMessage { get; set; }
    }
}
