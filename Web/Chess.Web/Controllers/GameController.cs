namespace Chess.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using Chess.Services.Data;

    public class GameController : BaseController
    {
        private readonly IGameService gameService;

        public GameController(IGameService gameService)
        {
            this.gameService = gameService;
        }

        public IActionResult Index()
        {
            return this.View();
        }

        public IActionResult New()
        {
            return this.View();
        }
    }
}
