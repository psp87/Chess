namespace Chess.Data.Models.EventArgs
{
    using System;

    public class TakePieceEventArgs : EventArgs
    {
        public TakePieceEventArgs(string name)
        {
            this.PieceName = name;
        }

        public string PieceName { get; set; }
    }
}
