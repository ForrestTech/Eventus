using System;
using System.Configuration;
using System.Threading.Tasks;
using EventSourcing.Samples.Infrastructure.DocumentDb;
using EventSourcing.Storage;

namespace EventSourcing.Samples.Infrastructure.EventStore
{
    public class EventStorageProviderFactory
    {
        public static Task<IEventStorageProvider> CreateAsync()
        {
            var provider = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    throw new NotImplementedException();
                case Constants.DocumentDb:
                    return Task.FromResult(DocumentDbFactory.CreateDocumentDbStorageProvider());
                default:
                    throw new ConfigurationErrorsException($"Unrecognised provider '{provider}' provide a valid provider");
            }
        }
    }
}