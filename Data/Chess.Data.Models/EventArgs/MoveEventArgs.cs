namespace Chess.Data.Models.EventArgs
{
    using System;

    using Chess.Common.Enums;

    public class MoveEventArgs : EventArgs
    {
        public MoveEventArgs(Notification type)
        {
            this.Type = type;
        }

        public Notification Type { get; set; }
    }
}
