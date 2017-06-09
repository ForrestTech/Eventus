using System.Threading.Tasks;
using Mailgun.Messages;
using Mailgun.Service;
using Microsoft.Extensions.Options;

namespace Eventus.Samples.Web.Services
{
    public class MailGunEmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;

        public MailGunEmailSender(IOptionsSnapshot<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var mg = new MessageService(_emailOptions.MailGunAPIKey);

            var mail = new MessageBuilder()
                .AddToRecipient(new Recipient
                {
                    Email = email
                })
                .SetSubject(subject)
                .SetFromAddress(new Recipient { Email = _emailOptions.EmailFrom, DisplayName = "Eventus" })
                .SetHtmlBody(message)
                .GetMessage();

            return mg.SendMessageAsync(_emailOptions.EmailDomain, mail);
        }
    }
}