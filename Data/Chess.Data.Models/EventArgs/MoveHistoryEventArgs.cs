namespace Chess.Data.Models.EventArgs
{
    using System;

    public class MoveHistoryEventArgs : EventArgs
    {
        public MoveHistoryEventArgs(string moveString)
        {
            this.MoveString = moveString;
        }

        public string MoveString { get; set; }
    }
}
