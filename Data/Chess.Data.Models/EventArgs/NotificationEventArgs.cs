namespace Chess.Data.Models.EventArgs
{
    using System;

    using Chess.Common.Enums;

    public class NotificationEventArgs : EventArgs
    {
        public NotificationEventArgs(Notification type)
        {
            this.Type = type;
        }

        public Notification Type { get; set; }
    }
}
