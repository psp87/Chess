namespace Chess.Services.Data.Models.EventArgs
{
    using System;

    public class ThreefoldDrawEventArgs : EventArgs
    {
        public ThreefoldDrawEventArgs(bool isAvailable)
        {
            this.IsAvailable = isAvailable;
        }

        public bool IsAvailable { get; set; }
    }
}
