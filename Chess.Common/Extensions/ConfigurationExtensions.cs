namespace Chess.Common.Extensions
{
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        public static string GetChessDbConnectionString(this IConfiguration configuration)
            => configuration.GetConnectionString("ChessDb");

        public static IConfigurationSection GetEmailConfigurationSection(this IConfiguration configuration)
            => configuration.GetSection("EmailSettings");
    }
}
