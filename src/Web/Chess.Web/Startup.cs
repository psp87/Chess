namespace Chess.Web
{
    using System.Reflection;

    using Chess.Common.Configuration;
    using Chess.Common.Extensions;
    using Chess.Data;
    using Chess.Data.Common;
    using Chess.Data.Common.Repositories;
    using Chess.Data.Models;
    using Chess.Data.Repositories;
    using Chess.Data.Seeding;
    using Chess.Services.Data.Services;
    using Chess.Services.Data.Services.Contracts;
    using Chess.Services.Mapping;
    using Chess.Services.Messaging;
    using Chess.Services.Messaging.Contracts;
    using Chess.Web.Hubs;
    using Chess.Web.ViewModels;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            this.AddDatabase(services);
            this.AddIdentity(services);
            this.AddCookiePolicy(services);
            this.AddControllers(services);
            this.AddSettings(services);
            this.AddSignalR(services);
            this.AddRepositories(services);
            this.AddServices(services);
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AutoMapperConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ChessDbContext>();

                if (env.IsDevelopment())
                {
                    // dbContext.Database.EnsureDeleted();
                    // dbContext.Database.EnsureCreated();
                    dbContext.Database.Migrate();
                }

                new ChessDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                    {
                        endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                        endpoints.MapRazorPages();
                        endpoints.MapHub<GameHub>("/hub");
                    });
        }

        private void AddDatabase(IServiceCollection services)
        {
            services.AddDbContext<ChessDbContext>(options => options
                .UseSqlServer(this.configuration.GetChessDbConnectionString()));
        }

        private void AddIdentity(IServiceCollection services)
        {
            services.AddDefaultIdentity<UserEntity>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<RoleEntity>().AddEntityFrameworkStores<ChessDbContext>();
        }

        private void AddCookiePolicy(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(
                            options =>
                            {
                                options.CheckConsentNeeded = context => true;
                                options.MinimumSameSitePolicy = SameSiteMode.None;
                            });
        }

        private void AddControllers(IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
        }

        private void AddSettings(IServiceCollection services)
        {
            services.Configure<EmailConfiguration>(this.configuration.GetEmailConfigurationSection());
        }

        private void AddSignalR(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<GameHub>();
        }

        private void AddRepositories(IServiceCollection services)
        {
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddTransient<IEmailSender>(x =>
                new SendGridEmailSender(this.configuration.GetValue<string>("SendGridApiKey")));
            services.AddTransient<IGameService, GameService>();
            services.AddTransient<IStatsService, StatsService>();
            services.AddTransient<IDrawService, DrawService>();
            services.AddTransient<ICheckService, CheckService>();
            services.AddTransient<IUtilityService, UtilityService>();
            services.AddSingleton<INotificationService, NotificationService>();
        }
    }
}
