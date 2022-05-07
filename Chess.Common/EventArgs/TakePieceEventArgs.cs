namespace Chess.Common.EventArgs
{
    using System;

    public class TakePieceEventArgs : EventArgs
    {
        public TakePieceEventArgs(string name, int points)
        {
            this.PieceName = name;
            this.Points = points;
        }

        public string PieceName { get; set; }

        public int Points { get; set; }
    }
}
