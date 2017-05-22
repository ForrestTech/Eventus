using System.Configuration;
using System.Threading.Tasks;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class StorageProviderInitialiser
    {
        public static Task InitAsync()
        {
            var provider = StorageProviderFactory.FromString(ConfigurationManager.AppSettings[Constants.Provider]);
            return provider.InitAsync();
        }
    }
}