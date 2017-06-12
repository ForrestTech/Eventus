using System.Configuration;
using System.Threading.Tasks;
using Eventus.Cleanup;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class TearDownFactory
    {
        public static async Task<ITeardown> CreateAsync()
        {
            var provider = ProviderFactory.FromString(ConfigurationManager.AppSettings[Constants.Provider].ToLowerInvariant());
            var repo = await provider.CreateTeardownAsync().ConfigureAwait(false);
            return repo;
        }
    }
}