<<<<<<< HEAD:Services/Chess.Services.Data/Models/EventArgs/TakePieceEventArgs.cs
﻿namespace Chess.Services.Data.Models.EventArgs
=======
﻿namespace Chess.Web.Models.EventArgs
>>>>>>> master:Web/Chess.Web/Models/EventArgs/TakePieceEventArgs.cs
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
