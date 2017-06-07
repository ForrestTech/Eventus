using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Mailgun;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Eventus.Samples.Web.Services
{
    public class MailGunEmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;

        public MailGunEmailSender(IOptionsSnapshot<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var sender = new MailgunSender(_emailOptions.EmailDomain, _emailOptions.MailGunAPIKey);
            Email.DefaultSender = sender;

            var mail = Email
                .From(_emailOptions.EmailFrom)
                .To(email)
                .Subject(subject)
                .Body(message, true);

            await mail.SendAsync().ConfigureAwait(false);
        }
    }

    public class TwilioSmsSender : ISmsSender
    {
        private readonly SmsOptions _smsOptions;

        public TwilioSmsSender(IOptionsSnapshot<SmsOptions> smsOptions)
        {
            _smsOptions = smsOptions.Value;
        }

        public Task SendSmsAsync(string number, string message)
        {
            var accountSid = _smsOptions.TwilioAccountSID;
            // Your Auth Token from twilio.com/console
            var authToken = _smsOptions.TwilioAuthToken;

            TwilioClient.Init(accountSid, authToken);

            var msg = MessageResource.Create(
                to: new PhoneNumber(number),
                from: new PhoneNumber(_smsOptions.TwilioPhoneNumberFrom),
                body: message);
            return Task.FromResult(0);
        }
    }

    public class EmailOptions
    {
        public string MailGunAPIKey { get; set; }

        public string EmailDomain { get; set; }

        public string EmailFrom { get; set; }
    }

    public class SmsOptions
    {
        public string TwilioAccountSID { get; set; }

        public string TwilioPhoneNumberFrom { get; set; }

        public string TwilioAuthToken { get; set; }
    }
}
