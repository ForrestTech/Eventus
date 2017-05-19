using System.Configuration;
using System.Threading.Tasks;
using Eventus.Samples.Infrastructure.DocumentDb;
using Eventus.Samples.Infrastructure.Factories;
using Eventus.Storage;

namespace Eventus.Samples.Infrastructure.EventStore
{
    public class EventStorageProviderFactory
    {
        public static Task<IEventStorageProvider> CreateAsync(bool addLogging = false)
        {
            var provider = ConfigurationManager.AppSettings["StorageProvider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    return EventStoreFactory.CreateEventStoreEventStorageProviderAsync(addLogging);
                case Constants.DocumentDb:
                    return DocumentDbFactory.CreateDocumentDbEventProviderAsync(false, addLogging);
                default:
                    throw new ConfigurationErrorsException($"Unrecognized provider '{provider}' provide a valid provider");
            }
        }
    }
}