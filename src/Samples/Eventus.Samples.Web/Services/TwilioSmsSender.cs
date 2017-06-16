using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Eventus.Samples.Web.Services
{
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
                @from: new PhoneNumber(_smsOptions.TwilioPhoneNumberFrom),
                body: message);
            return Task.FromResult(0);
        }
    }
}