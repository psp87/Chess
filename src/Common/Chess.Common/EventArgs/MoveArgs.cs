namespace Chess.Common.EventArgs
{
    using System;

    using Chess.Common.Enums;

    public class MoveArgs : EventArgs
    {
        public MoveArgs(Message type)
        {
            this.Type = type;
        }

        public Message Type { get; set; }
    }
}
