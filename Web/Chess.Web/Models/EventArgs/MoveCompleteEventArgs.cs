<<<<<<< HEAD:Services/Chess.Services.Data/Models/EventArgs/MoveCompleteEventArgs.cs
﻿namespace Chess.Services.Data.Models.EventArgs
=======
﻿namespace Chess.Web.Models.EventArgs
>>>>>>> master:Web/Chess.Web/Models/EventArgs/MoveCompleteEventArgs.cs
{
    using System;

    public class MoveCompleteEventArgs : EventArgs
    {
        public MoveCompleteEventArgs(string notation)
        {
            this.Notation = notation;
        }

        public string Notation { get; set; }
    }
}
