using System.Configuration;
using System.Threading.Tasks;
using Eventus.Storage;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class EventStorageProviderFactory
    {
        public static async Task<IEventStorageProvider> CreateAsync()
        {
            var provider = StorageProviderFactory.FromString(ConfigurationManager.AppSettings[Constants.Provider].ToLowerInvariant());
            var repo = await provider.CreateEventStorageProviderAsync().ConfigureAwait(false);
            return repo;
        }
    }
}