namespace Chess.Data.Models
{
    using System;
    using System.Collections.Generic;

    using Chess.Data.Models.Enums;
    using Chess.Data.Models.Pieces;

    public class Player
    {
        private Dictionary<string, int> takenFigures;

        public Player(string name, Color color)
        {
            this.Id = Guid.NewGuid().ToString();
            this.UserName = name;
            this.Color = color;
            this.IsMoveAvailable = true;
            this.IsCheck = false;
            this.IsCheckmate = false;
            this.takenFigures = new Dictionary<string, int>()
            {
                { nameof(Pawn), 0 },
                { nameof(Knight), 0 },
                { nameof(Bishop), 0 },
                { nameof(Rook), 0 },
                { nameof(Queen), 0 },
            };
            this.InitializeMove();
        }

        public string Id { get; set; }

        public string GameId { get; set; }

        public string UserName { get; set; }

        public Color Color { get; }

        public bool IsCheck { get; set; }

        public bool IsCheckmate { get; set; }

        public bool IsMoveAvailable { get; set; }

        public bool HasToMove { get; set; }

        public int Points { get; set; }

        public void TakeFigure(string figureName)
        {
            this.takenFigures[figureName]++;
        }

        public void InitializeMove()
        {
            if (this.Color == Color.Light)
            {
                this.HasToMove = true;
            }
        }
    }
}
