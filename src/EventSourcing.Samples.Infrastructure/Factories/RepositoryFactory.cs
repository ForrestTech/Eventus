using System.Configuration;
using System.Threading.Tasks;
using EventSourcing.Samples.Infrastructure.DocumentDb;
using EventSourcing.Storage;

namespace EventSourcing.Samples.Infrastructure.Factories
{
    public class RepositoryFactory
    {
        public static Task<IRepository> CreateAsync(bool addLogging = false)
        {
            var provider = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    return EventStoreFactory.CreateEventStoreRepositoryAsync(addLogging);
                case Constants.DocumentDb:
                    return DocumentDbFactory.CreateDocumentDbRepositoryAsync(addLogging);
                default:
                    throw new ConfigurationErrorsException($"Unrecognized provider '{provider}' provide a valid provider");
            }
        }
    }
}
