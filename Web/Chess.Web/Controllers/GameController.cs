namespace Chess.Web.Controllers
{
    using Chess.Services.Data.Contracts;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class GameController : BaseController
    {
        private readonly IGameService gameService;

        public GameController(IGameService gameService)
        {
            this.gameService = gameService;
        }

        public IActionResult Index()
        {
            return this.View(this.User.Identity);
        }

        public IActionResult New()
        {
            return this.View();
        }
    }
}
