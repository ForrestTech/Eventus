using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Mailgun;
using Microsoft.Extensions.Options;

namespace Eventus.Samples.Web.Services
{
    public class MailGunEmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;

        public MailGunEmailSender(IOptionsSnapshot<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;

            var sender = new MailgunSender(
                _emailOptions.EmailDomain,
                _emailOptions.MailGunAPIKey
            );

            Email.DefaultSender = sender;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var mail = Email
                .From(_emailOptions.EmailFrom)
                .To(email)
                .Subject(subject)
                .Body(message, true);

            return mail.SendAsync();
        }
    }
}