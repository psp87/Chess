namespace Chess.Data.Models.EventArgs
{
    using System;

    using Chess.Common.Enums;

    public class CheckEventArgs : EventArgs
    {
        public CheckEventArgs(Check check)
        {
            this.Check = check;
        }

        public Check Check { get; set; }
    }
}
