namespace Chess.Data.Configurations
{
    using Chess.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StatisticConfiguration : IEntityTypeConfiguration<Statistic>
    {
        public void Configure(EntityTypeBuilder<Statistic> builder)
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
                .Property(x => x.Games)
                .HasColumnName("games");
            builder
                .Property(x => x.Win)
                .HasColumnName("win");
            builder
                .Property(x => x.Draw)
                .HasColumnName("draw");
            builder
                .Property(x => x.Loss)
                .HasColumnName("loss");
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
