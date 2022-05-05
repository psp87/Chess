namespace Chess.Data.Configurations
{
    using Chess.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StatisticConfiguration : IEntityTypeConfiguration<StatisticEntity>
    {
        public void Configure(EntityTypeBuilder<StatisticEntity> builder)
        {
            builder
                .ToTable("stats");

            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.Played)
                .HasColumnName("played")
                .IsRequired();

            builder
                .Property(x => x.Won)
                .HasColumnName("won")
                .IsRequired();

            builder
                .Property(x => x.Drawn)
                .HasColumnName("drawn")
                .IsRequired();

            builder
                .Property(x => x.Lost)
                .HasColumnName("lost")
                .IsRequired();

            builder
                .Property(x => x.EloRating)
                .HasColumnName("elo_rating")
                .IsRequired();

            builder
                .Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder
                .Property(x => x.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired();

            builder
                .Property(x => x.ModifiedOn)
                .HasColumnName("modified_on");
        }
    }
}
