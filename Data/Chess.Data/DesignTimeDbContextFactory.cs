namespace Chess.Data
{
    using System.IO;

    using Chess.Common.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ChessDbContext>
    {
        public ChessDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
                .Build();

            var builder = new DbContextOptionsBuilder<ChessDbContext>();
            builder.UseSqlServer(configuration.GetChessDbConnectionString());

            return new ChessDbContext(builder.Options);
        }
    }
}
