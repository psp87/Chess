namespace Chess.Services.Data.Models.EventArgs
{
    using System;

    public class MoveCompleteEventArgs : EventArgs
    {
        public MoveCompleteEventArgs(string notation)
        {
            this.Notation = notation;
        }

        public string Notation { get; set; }
    }
}
