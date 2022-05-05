namespace Chess.Data.Configurations
{
    using Chess.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLogEntity>
    {
        public void Configure(EntityTypeBuilder<ErrorLogEntity> builder)
        {
            builder
                .ToTable("error_logs");

            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired();

            builder
                .Property(x => x.GameId)
                .HasColumnName("game_id")
                .IsRequired();

            builder
                .Property(x => x.Source)
                .HasColumnName("source")
                .IsRequired();

            builder
                .Property(x => x.Target)
                .HasColumnName("target")
                .IsRequired();

            builder
                .Property(x => x.FenString)
                .HasColumnName("fen_string")
                .IsRequired();

            builder
                .Property(x => x.ExceptionMessage)
                .HasColumnName("exception_message")
                .IsRequired();
        }
    }
}
