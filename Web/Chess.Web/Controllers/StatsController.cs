namespace Chess.Web.Controllers
{
    using Chess.Services.Data.Contracts;
    using Microsoft.AspNetCore.Mvc;

    public class StatsController : BaseController
    {
        private readonly IStatsService statsService;

        public StatsController(IStatsService statsService)
        {
            this.statsService = statsService;
        }

        public IActionResult Index()
        {
            return this.View();
        }
    }
}
