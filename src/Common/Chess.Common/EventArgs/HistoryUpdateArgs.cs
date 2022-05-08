namespace Chess.Common.EventArgs
{
    using System;

    public class HistoryUpdateArgs : EventArgs
    {
        public HistoryUpdateArgs(string notation)
        {
            this.Notation = notation;
        }

        public string Notation { get; set; }
    }
}
