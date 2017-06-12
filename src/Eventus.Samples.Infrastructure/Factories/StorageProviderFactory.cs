using System.Configuration;
using System.Threading.Tasks;
using Eventus.Storage;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class StorageProviderFactory
    {
        public static async Task<IEventStorageProvider> CreateAsync()
        {
            var provider = ProviderFactory.FromString(ConfigurationManager.AppSettings[Constants.Provider].ToLowerInvariant());
            var repo = await provider.CreateEventStorageProviderAsync().ConfigureAwait(false);
            return repo;
        }
    }
}