using System.Threading.Tasks;

namespace Eventus.Samples.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
