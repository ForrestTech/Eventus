using System.Configuration;
using EventSourcing.Storage;

namespace EventSourcing.Samples.Infrastructure
{
    public class RepositoryFactory
    {
        public static Repository Create()
        {
            var provider = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (provider)
            {
                case "eventstore":
                    return EventStoreFactory.CreateEventStoreRepository();
                case "documentdb":
                    return DocumentDbFactory.CreateDocumentDbRepository();
                default:
                    throw new ConfigurationErrorsException($"Unrecognised provider '{provider}' provide a valid provider");
            }
        }
    }
}
