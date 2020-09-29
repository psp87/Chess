namespace Chess.Data.Models.EventArgs
{
    using System;

    using Chess.Common.Enums;

    public class MoveEventArgs : EventArgs
    {
        public MoveEventArgs(Message type)
        {
            this.Type = type;
        }

        public Message Type { get; set; }
    }
}
