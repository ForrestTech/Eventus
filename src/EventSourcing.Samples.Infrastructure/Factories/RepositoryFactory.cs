using System.Configuration;
using System.Threading.Tasks;
using EventSourcing.Samples.Infrastructure.DocumentDb;
using EventSourcing.Storage;

namespace EventSourcing.Samples.Infrastructure.Factories
{
    public class RepositoryFactory
    {
        public static Task<Repository> CreateAsync()
        {
            var provider = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    return EventStoreFactory.CreateEventStoreRepository();
                case Constants.DocumentDb:
                    return DocumentDbFactory.CreateDocumentDbRepository();
                default:
                    throw new ConfigurationErrorsException($"Unrecognised provider '{provider}' provide a valid provider");
            }
        }
    }
}
