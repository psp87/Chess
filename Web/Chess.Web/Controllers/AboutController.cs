namespace Chess.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class AboutController : BaseController
    {
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
