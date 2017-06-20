using System.Threading.Tasks;

namespace Eventus.Samples.Web.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
