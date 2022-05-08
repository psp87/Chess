using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Chess.Web.Areas.Identity.IdentityHostingStartup))]

namespace Chess.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}
