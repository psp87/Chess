<<<<<<< HEAD:Services/Chess.Services.Data/Models/EventArgs/MoveEventArgs.cs
﻿namespace Chess.Services.Data.Models.EventArgs
=======
﻿namespace Chess.Web.Models.EventArgs
>>>>>>> master:Web/Chess.Web/Models/EventArgs/MoveEventArgs.cs
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
