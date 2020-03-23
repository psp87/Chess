namespace Chess.Web.Controllers
{
    using Chess.Services.Data;

    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

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
