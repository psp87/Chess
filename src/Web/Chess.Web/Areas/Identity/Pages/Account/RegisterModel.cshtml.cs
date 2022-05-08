namespace Chess.Web.Areas.Identity.Pages.Account
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    using Chess.Common.Configuration;
    using Chess.Common.Constants;
    using Chess.Data.Models;
    using Chess.Services.Messaging;
    using Chess.Services.Messaging.Contracts;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<UserEntity> signInManager;
        private readonly UserManager<UserEntity> userManager;
        private readonly ILogger<RegisterModel> logger;
        private readonly IEmailSender emailSender;
        private readonly EmailConfiguration configuration;

        public RegisterModel(
            UserManager<UserEntity> userManager,
            SignInManager<UserEntity> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IOptions<EmailConfiguration> options)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.emailSender = emailSender;
            this.configuration = options.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            this.ReturnUrl = returnUrl;
            this.ExternalLogins = (await this.signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= this.Url.Content("~/");
            this.ExternalLogins = (await this.signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (this.ModelState.IsValid)
            {
                var user = new UserEntity { UserName = this.Input.Email, Email = this.Input.Email };
                var result = await this.userManager.CreateAsync(user, this.Input.Password);
                if (result.Succeeded)
                {
                    this.logger.LogInformation("User created a new account with password.");

                    await this.emailSender.SendEmailAsync(
                        new MailMessageBuilder()
                        .From(this.configuration.MyAbvMail)
                        .FromName(MailConstants.MyName)
                        .To(this.Input.Email)
                        .Subject(MailConstants.Subject)
                        .HtmlContent(MailConstants.Body)
                        .Build());

                    await this.emailSender.SendEmailAsync(
                        new MailMessageBuilder()
                        .From(this.configuration.MyAbvMail)
                        .FromName(MailConstants.MyName)
                        .To(this.configuration.MyGmail)
                        .Subject("My-chess Registration")
                        .HtmlContent($"New user ({this.Input.Email}) has been just registered.")
                        .Build());

                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    return this.LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return this.Page();
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
    }
}
