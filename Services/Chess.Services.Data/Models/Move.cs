namespace Chess.Services.Data.Models
{
    using Chess.Common.Enums;
    using Chess.Services.Data.Models.Pieces.Helpers;

    public class Move
    {
        public Move()
        {
        }

        public Move(Square source, Square target)
        {
            this.Source = source;
            this.Target = target;
        }

        public Square Source { get; set; } = Factory.GetSquare();

        public Square Target { get; set; } = Factory.GetSquare();

        public MoveType Type { get; set; } = MoveType.Normal;

        public CastlingArgs CastlingArgs { get; set; } = Factory.GetCastlingArgs();

        public EnPassantArgs EnPassantArgs { get; set; } = Factory.GetEnPassantArgs();

        public PawnPromotionArgs PawnPromotionArgs { get; set; } = Factory.GetPawnPromotionArgs();

        public override string ToString()
            => this.Source.ToString() + this.Target.ToString();
    }
}
