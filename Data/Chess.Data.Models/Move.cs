namespace Chess.Data.Models
{
    using Chess.Common.Enums;
    using Chess.Data.Models.Pieces.Helpers;

    public class Move
    {
        public Move()
        {
            this.Source = Factory.GetSquare();
            this.Target = Factory.GetSquare();

            this.CastlingArgs = new Castling();
            this.EnPassantArgs = new EnPassant();
            this.PawnPromotionArgs = new PawnPromotion();

            this.Type = MoveType.Normal;
        }

        public Square Source { get; set; }

        public Square Target { get; set; }

        public MoveType Type { get; set; }

        public Castling CastlingArgs { get; set; }

        public EnPassant EnPassantArgs { get; set; }

        public PawnPromotion PawnPromotionArgs { get; set; }

        public override string ToString()
        {
            return this.Source.ToString() + this.Target.ToString();
        }
    }
}
