<<<<<<< HEAD:Services/Chess.Services.Data/Models/EventArgs/ThreefoldDrawEventArgs.cs
﻿namespace Chess.Services.Data.Models.EventArgs
=======
﻿namespace Chess.Web.Models.EventArgs
>>>>>>> master:Web/Chess.Web/Models/EventArgs/ThreefoldDrawEventArgs.cs
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
