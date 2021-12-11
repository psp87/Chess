namespace Common.Extensions
{
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationExtensions
    {
        public static IConfigurationSection GetEmailConfigurationSection(this IConfiguration configuration)
            => configuration.GetSection("EmailSettings");
    }
}
