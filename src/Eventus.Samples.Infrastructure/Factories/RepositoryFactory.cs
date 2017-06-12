using System.Configuration;
using System.Threading.Tasks;
using Eventus.Storage;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class RepositoryFactory
    {
        public static async Task<IRepository> CreateAsync()
        {
            var provider = ProviderFactory.FromString(ConfigurationManager.AppSettings[Constants.Provider].ToLowerInvariant());
            var repo = await provider.CreateRepositoryAsync().ConfigureAwait(false);
            return repo;
        }
    }
}
