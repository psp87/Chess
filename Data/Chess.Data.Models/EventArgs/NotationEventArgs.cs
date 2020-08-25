namespace Chess.Data.Models.EventArgs
{
    using System;

    public class NotationEventArgs : EventArgs
    {
        public NotationEventArgs(string notation)
        {
            this.Notation = notation;
        }

        public string Notation { get; set; }
    }
}
