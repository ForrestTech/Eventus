using System.Configuration;
using System.Threading.Tasks;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class StorageProviderInitialiser
    {
        public static Task InitAsync()
        {
            var provider = ProviderFactory.FromString(ConfigurationManager.AppSettings[Constants.Provider]);
            return provider.InitAsync();
        }
    }
}