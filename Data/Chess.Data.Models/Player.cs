namespace Chess.Models
{
    using System.Collections.Generic;

    using Enums;
    using Pieces;

    public class Player
    {
        private Dictionary<string, int> takenFigures;

        public Player(string name, Color color)
        {
            this.takenFigures = new Dictionary<string, int>()
            {
                { nameof(Pawn), 0 },
                { nameof(Knight), 0 },
                { nameof(Bishop), 0 },
                { nameof(Rook), 0 },
                { nameof(Queen), 0 },
            };
            this.Name = name;
            this.Color = color;
            this.IsMoveAvailable = true;
        }

        public string Name { get; }

        public Color Color { get; }

        public bool HasToMove { get; set; }

        public bool IsCheck { get; set; }

        public bool IsMoveAvailable { get; set; }

        public void TakeFigure(string figureName)
        {
            this.takenFigures[figureName]++;
        }

        public int TakenFigures(string figureName)
        {
            return this.takenFigures[figureName];
        }
    }
}
