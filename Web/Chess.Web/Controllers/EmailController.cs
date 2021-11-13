namespace Chess.Web.Controllers
{
    using System.Threading.Tasks;

    using Chess.Services.Messaging.Contracts;
    using Microsoft.AspNetCore.Mvc;

    public class EmailController : BaseController
    {
        private readonly IEmailSender sender;

        public EmailController(IEmailSender sender)
        {
            this.sender = sender;
        }

        public async Task<IActionResult> Send(string email)
        {
            await this.sender
                .SendEmailAsync("psp87@abv.bg", "Plamen Petrov ", email, "Successful Registration", "Enjoy playing chess!");

            return this.View("/Views/Home/Index.cshtml");
        }
    }
}
