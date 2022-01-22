namespace Chess.Data.Configurations
{
    using Chess.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MoveConfiguration : IEntityTypeConfiguration<Move>
    {
        public void Configure(EntityTypeBuilder<Move> builder)
        {
            builder
                .ToTable("moves");
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            builder
                .Property(x => x.GameId)
                .HasColumnName("game_id")
                .IsRequired();

            builder
                .HasOne(x => x.Game)
                .WithMany(x => x.Moves)
                .HasForeignKey(x => x.GameId);
        }
    }
}
