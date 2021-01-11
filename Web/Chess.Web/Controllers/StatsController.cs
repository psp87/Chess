namespace Chess.Web.Controllers
{
    using System.Security.Claims;
    using System.Threading;

    using Chess.Data.Models;
    using Chess.Services.Data.Contracts;
    using Chess.Web.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class StatsController : BaseController
    {
        private readonly IStatsService statsService;

        public StatsController(IStatsService statsService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.statsService = statsService;
        }

        public IActionResult Index()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var stats = this.statsService.GetUserStats(userId);

            if (stats == null)
            {
                this.statsService.InitiateStats(userId);
                Thread.Sleep(100);
            }

            var userStats = this.statsService.GetUserStats<UserStatsViewModel>(userId);

            var model = new StatsViewModel
            {
                Name = this.User.Identity.Name,
                UserStats = userStats,
                TotalUsers = this.statsService.GetTotalUsers(),
                TotalMatches = this.statsService.GetTotalMatches(),
                MostMatchesUser = this.statsService.GetMostMatchesUser(),
                MostWonsUser = this.statsService.GetMostWonsUser(),
            };

            return this.View(model);
        }
    }
}
