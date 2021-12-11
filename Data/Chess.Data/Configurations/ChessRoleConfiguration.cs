namespace Chess.Data.Configurations
{
    using Chess.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ChessRoleConfiguration : IEntityTypeConfiguration<ChessRole>
    {
        public void Configure(EntityTypeBuilder<ChessRole> builder)
        {
            builder
                .ToTable("roles");
        }
    }
}
