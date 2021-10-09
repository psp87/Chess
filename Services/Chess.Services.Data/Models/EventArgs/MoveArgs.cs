namespace Chess.Services.Data.Models.EventArgs
{
    using System;

    using Common.Enums;

    public class MoveArgs : EventArgs
    {
        public MoveArgs(Message type)
        {
            this.Type = type;
        }

        public Message Type { get; set; }
    }
}
