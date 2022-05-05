namespace Chess.Data.Configurations
{
    using Chess.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GameConfiguration : IEntityTypeConfiguration<GameEntity>
    {
        public void Configure(EntityTypeBuilder<GameEntity> builder)
        {
            builder
                .ToTable("games");

            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder
                .Property(x => x.PlayerOneName)
                .HasColumnName("player_one_name")
                .IsRequired();

            builder
                .Property(x => x.PlayerOneUserId)
                .HasColumnName("player_one_user_id")
                .IsRequired();

            builder
                .Property(x => x.PlayerTwoName)
                .HasColumnName("player_two_name")
                .IsRequired();

            builder
                .Property(x => x.PlayerTwoUserId)
                .HasColumnName("player_two_user_id")
                .IsRequired();

            builder
                .Property(x => x.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired();

            builder
                .Property(x => x.ModifiedOn)
                .HasColumnName("modified_on");

            builder
                .HasMany(x => x.Moves)
                .WithOne(x => x.Game)
                .HasForeignKey(x => x.GameId);
        }
    }
}
