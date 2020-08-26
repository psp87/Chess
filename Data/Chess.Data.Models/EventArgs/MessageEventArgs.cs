namespace Chess.Data.Models.EventArgs
{
    using System;

    using Chess.Common.Enums;

    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(Notification type)
        {
            this.Type = type;
        }

        public Notification Type { get; set; }
    }
}
