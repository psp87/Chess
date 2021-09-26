<<<<<<<< HEAD:Services/Chess.Services.Data/Models/EventArgs/GameOverEventArgs.cs
﻿namespace Chess.Services.Data.Models.EventArgs
========
<<<<<<< HEAD:Services/Chess.Services.Data/Models/EventArgs/GameOverEventArgs.cs
﻿namespace Chess.Services.Data.Models.EventArgs
=======
﻿namespace Chess.Web.Models.EventArgs
>>>>>>> master:Web/Chess.Web/Models/EventArgs/GameOverEventArgs.cs
>>>>>>>> master:Web/Chess.Web/Models/EventArgs/GameOverEventArgs.cs
{
    using System;

    using Chess.Common.Enums;

    public class GameOverEventArgs : EventArgs
    {
        public GameOverEventArgs(GameOver gameOver)
        {
            this.GameOver = gameOver;
        }

        public GameOver GameOver { get; set; }
    }
}
