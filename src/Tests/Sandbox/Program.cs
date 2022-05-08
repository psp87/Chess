namespace Sandbox
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Chess.Common.Extensions;
    using Chess.Data;
    using Chess.Data.Common;
    using Chess.Data.Common.Repositories;
    using Chess.Data.Models;
    using Chess.Data.Repositories;
    using Chess.Data.Seeding;
    using CommandLine;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine($"{typeof(Program).Namespace} ({string.Join(" ", args)}) starts working...");

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(true);

            using (var serviceScope = serviceProvider.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ChessDbContext>();
                context.Database.Migrate();
                new ChessDbContextSeeder()
                    .SeedAsync(context, serviceScope.ServiceProvider)
                    .GetAwaiter()
                    .GetResult();
            }

            using (var serviceScope = serviceProvider.CreateScope())
            {
                serviceProvider = serviceScope.ServiceProvider;

                return Parser.Default
                    .ParseArguments<SandboxOptions>(args)
                    .MapResult(
                        opts => SandboxCode(opts, serviceProvider)
                            .GetAwaiter()
                            .GetResult(),
                        _ => 255);
            }
        }

        private static async Task<int> SandboxCode(SandboxOptions options, IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ChessDbContext>();
            var stats = await context.Stats
                .Include(x => x.User)
                .ToListAsync();

            stats.ForEach(x => Console.WriteLine($"User: {x.User.UserName}, Games: {x.Played}, Rating: {x.EloRating}"));

            return await Task.FromResult(0);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            services.AddDbContext<ChessDbContext>(
                options => options
                    .UseSqlServer(configuration.GetChessDbConnectionString())
                    .UseLoggerFactory(new LoggerFactory()));

            services
                .AddDefaultIdentity<UserEntity>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<RoleEntity>()
                .AddEntityFrameworkStores<ChessDbContext>();

            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        }
    }
}
