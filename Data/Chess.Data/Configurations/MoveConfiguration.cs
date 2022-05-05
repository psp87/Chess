namespace Chess.Data.Configurations
{
    using Chess.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class MoveConfiguration : IEntityTypeConfiguration<MoveEntity>
    {
        public void Configure(EntityTypeBuilder<MoveEntity> builder)
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
                .Property(x => x.Notation)
                .HasColumnName("notation")
                .IsRequired();

            builder
                .Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder
                .Property(x => x.GameId)
                .HasColumnName("game_id")
                .IsRequired();

            builder
                .Property(x => x.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired();

            builder
                .Property(x => x.ModifiedOn)
                .HasColumnName("modified_on");

            builder
                .HasOne(x => x.Game)
                .WithMany(x => x.Moves)
                .HasForeignKey(x => x.GameId);

            builder
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);
        }
    }
}
