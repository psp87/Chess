namespace Chess.Services.Messaging
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Chess.Services.Messaging.Contracts;
    using SendGrid;
    using SendGrid.Helpers.Mail;

    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridClient client;

        public SendGridEmailSender(string apiKey)
        {
            this.client = new SendGridClient(apiKey);
        }

        public async Task SendEmailAsync(MailMessage mailMessage)
        {
            if (string.IsNullOrWhiteSpace(mailMessage.Subject) && string.IsNullOrWhiteSpace(mailMessage.HtmlContent))
            {
                throw new ArgumentException("Subject and message should be provided.");
            }

            var fromAddress = new EmailAddress(mailMessage.From, mailMessage.FromName);
            var toAddress = new EmailAddress(mailMessage.To);
            var message = MailHelper.CreateSingleEmail(fromAddress, toAddress, mailMessage.Subject, null, mailMessage.HtmlContent);
            if (mailMessage.Attachments?.Any() == true)
            {
                foreach (var attachment in mailMessage.Attachments)
                {
                    message.AddAttachment(attachment.FileName, Convert.ToBase64String(attachment.Content), attachment.MimeType);
                }
            }

            try
            {
                var response = await this.client.SendEmailAsync(message);
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(await response.Body.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
