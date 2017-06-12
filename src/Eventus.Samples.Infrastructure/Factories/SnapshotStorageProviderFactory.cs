using System.Configuration;
using System.Threading.Tasks;
using Eventus.Storage;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class SnapshotStorageProviderFactory
    {
        public static async Task<ISnapshotStorageProvider> CreateAsync()
        {
            var provider = ProviderFactory.FromString(ConfigurationManager.AppSettings[Constants.Provider].ToLowerInvariant());
            var repo = await provider.CreateSnapshotStorageProviderAsync().ConfigureAwait(false);
            return repo;
        }
    }
}