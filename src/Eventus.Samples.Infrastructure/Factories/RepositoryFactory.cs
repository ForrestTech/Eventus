using System.Configuration;
using System.Threading.Tasks;
using Eventus.Samples.Infrastructure.DocumentDb;
using Eventus.Storage;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class RepositoryFactory
    {
        public static Task<IRepository> CreateAsync(bool initProvider = false, bool addLogging = false)
        {
            var provider = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    return EventStoreFactory.CreateEventStoreRepositoryAsync(addLogging);
                case Constants.DocumentDb:
                    return DocumentDbFactory.CreateDocumentDbRepositoryAsync(initProvider, addLogging);
                default:
                    throw new ConfigurationErrorsException($"Unrecognized provider '{provider}' provide a valid provider");
            }
        }
    }
}
