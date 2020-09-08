namespace Chess.Web.Controllers
{
    using System.Threading.Tasks;

    using Chess.Data.Models;
    using Chess.Services.Data.Contracts;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class GameController : BaseController
    {
        private readonly IGameService gameService;
        private readonly UserManager<ApplicationUser> userManager;

        public GameController(IGameService gameService, UserManager<ApplicationUser> userManager)
        {
            this.gameService = gameService;
            this.userManager = userManager;
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
