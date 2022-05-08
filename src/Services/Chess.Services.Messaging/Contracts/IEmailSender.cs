namespace Chess.Services.Messaging.Contracts
{
    using System.Threading.Tasks;

    public interface IEmailSender
    {
        Task SendEmailAsync(MailMessage mailMessage);
    }
}
