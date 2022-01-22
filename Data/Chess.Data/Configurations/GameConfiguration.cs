namespace Chess.Data.Configurations
{
    using Chess.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder
                .ToTable("games");
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .HasColumnName("id");
            builder
                .Property(x => x.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired();
            builder
                .Property(x => x.ModifiedOn)
                .HasColumnName("modified_on");
            builder
                .Property(x => x.Player1)
                .HasColumnName("player_1")
                .IsRequired();
            builder
                .Property(x => x.Player2)
                .HasColumnName("player_2")
                .IsRequired();
        }
    }
}
