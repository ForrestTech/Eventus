using System.Threading.Tasks;
using Eventus.Config;

namespace Eventus.Storage
{
    public interface IInitialiseStorageProvider
    {
        Task InitAsync(ProviderConfig config);
    }
}