namespace Chess.Data.Models
{
    using System.Collections.Generic;

    using Chess.Common.Enums;
    using Chess.Data.Models.Pieces;

    public class Player
    {
        private Dictionary<string, int> takenFigures;

        public Player(string username, string connectionId)
        {
            this.takenFigures = new Dictionary<string, int>()
            {
                { nameof(Pawn), 0 },
                { nameof(Knight), 0 },
                { nameof(Bishop), 0 },
                { nameof(Rook), 0 },
                { nameof(Queen), 0 },
            };
            this.Name = username;
            this.Id = connectionId;
        }

        public string Id { get; set; }

        public string Name { get; }

        public Color Color { get; set; }

        public string GameId { get; set; }

        public bool HasToMove { get; set; }

        public bool IsCheck { get; set; }

        public bool IsCheckMate { get; set; }

        public bool IsMoveAvailable { get; set; }

        public int Points { get; set; }

        public Dictionary<string, int> TakenFigures => this.takenFigures;

        public void TakeFigure(string figureName)
        {
            this.takenFigures[figureName]++;
        }
    }
}
