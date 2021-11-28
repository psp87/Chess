namespace Chess.Services.Messaging
{
    using System.Collections.Generic;

    public class MailMessage
    {
        public string From { get; set; }

        public string FromName { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string HtmlContent { get; set; }

        public IEnumerable<EmailAttachment> Attachments { get; set; }
    }
}
