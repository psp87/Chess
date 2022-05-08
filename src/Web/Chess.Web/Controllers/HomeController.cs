namespace Chess.Web.Controllers
{
    using System.Diagnostics;
    using System.Security.Claims;

    using Chess.Services.Data.Services.Contracts;
    using Chess.Web.ViewModels;
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : BaseController
    {
        private readonly IStatsService statsService;

        public HomeController(IStatsService statsService)
        {
            this.statsService = statsService;
        }

        public IActionResult Index()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!this.statsService.IsStatsInitiated(userId))
                {
                    this.statsService.InitiateStats(userId);
                }
            }

            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(
                new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
