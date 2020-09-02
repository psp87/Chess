namespace Chess.Web.Controllers
{
    using Chess.Data.Models;
    using Chess.Services.Data.Contracts;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            var user = await this.userManager.GetUserAsync(this.User);

            return this.View(user);
        }

        public IActionResult New()
        {
            return this.View();
        }
    }
}
