namespace Chess.Services.Messaging
{
    using System.Collections.Generic;

    public class MailMessageBuilder
    {
        private MailMessage message;

        public MailMessageBuilder()
        {
            this.message = new MailMessage();
        }

        public MailMessageBuilder From(string from)
        {
            this.message.From = from;
            return this;
        }

        public MailMessageBuilder FromName(string fromName)
        {
            this.message.FromName = fromName;
            return this;
        }

        public MailMessageBuilder To(string to)
        {
            this.message.To = to;
            return this;
        }

        public MailMessageBuilder Subject(string subject)
        {
            this.message.Subject = subject;
            return this;
        }

        public MailMessageBuilder HtmlContent(string htmlContent)
        {
            this.message.HtmlContent = htmlContent;
            return this;
        }

        public MailMessageBuilder Attachments(IEnumerable<EmailAttachment> attachments)
        {
            this.message.Attachments = attachments;
            return this;
        }

        public MailMessage Build() => this.message;
    }
}
