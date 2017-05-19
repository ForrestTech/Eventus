using System.Configuration;
using System.Threading.Tasks;
using Eventus.Samples.Infrastructure.DocumentDb;
using Eventus.Storage;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class SnapshotStorageProviderFactory
    {
        public static Task<ISnapshotStorageProvider> CreateAsync(bool addLogging = false)
        {
            var provider = ConfigurationManager.AppSettings["StorageProvider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    return EventStoreFactory.CreateSnapshotStorageProviderAsync(addLogging);
                case Constants.DocumentDb:
                    return DocumentDbFactory.CreateDocumentDbSnapshotProviderAsync(addLogging);
                default:
                    throw new ConfigurationErrorsException($"Unrecognized provider '{provider}' provide a valid provider");
            }
        }
    }
}