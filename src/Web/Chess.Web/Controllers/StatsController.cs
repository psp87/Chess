namespace Chess.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;

    using Chess.Data.Models;
    using Chess.Services.Data.Services.Contracts;
    using Chess.Web.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class StatsController : BaseController
    {
        private readonly IStatsService statsService;
        private readonly UserManager<UserEntity> userManager;

        public StatsController(IStatsService statsService, UserManager<UserEntity> userManager)
        {
            this.statsService = statsService;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var stats = this.statsService.GetUserStats<UserStatsViewModel>(userId);

            if (stats == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            var model = new StatsViewModel
            {
                UserName = this.User.Identity.Name,
                UserStats = stats,
                TotalUsers = this.userManager.Users.Count(),
                LastThirtyDaysRegisteredUsers = this.userManager.Users.Where(x => x.CreatedOn >= DateTime.Now.AddDays(-30)).Count(),
                TotalGames = this.statsService.GetTotalGames(),
                MostGamesUser = this.statsService.GetMostGamesUser(),
                MostWinsUser = this.statsService.GetMostWinsUser(),
            };

            return this.View(model);
        }
    }
}
