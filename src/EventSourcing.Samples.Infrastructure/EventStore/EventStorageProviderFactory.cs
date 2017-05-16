using System;
using System.Configuration;
using System.Threading.Tasks;
using EventSourcing.Samples.Infrastructure.DocumentDb;
using EventSourcing.Samples.Infrastructure.Factories;
using EventSourcing.Storage;

namespace EventSourcing.Samples.Infrastructure.EventStore
{
    public class EventStorageProviderFactory
    {
        public static Task<IEventStorageProvider> CreateAsync(bool addLogging = false)
        {
            var provider = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    return EventStoreFactory.CreateEventStoreEventStorageProviderAsync(addLogging);
                case Constants.DocumentDb:
                    return DocumentDbFactory.CreateDocumentDbEventProviderAsync(addLogging);
                default:
                    throw new ConfigurationErrorsException($"Unrecognized provider '{provider}' provide a valid provider");
            }
        }
    }
}