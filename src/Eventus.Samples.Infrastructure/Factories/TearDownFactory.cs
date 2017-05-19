using System.Configuration;
using Eventus.Cleanup;
using Eventus.EventStore;
using Eventus.Samples.Infrastructure.DocumentDb;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class TearDownFactory
    {
        public static ITeardown Create()
        {
            var provider = ConfigurationManager.AppSettings["StorageProvider"].ToLowerInvariant();
            switch (provider)
            {
                case Constants.Eventstore:
                    return new EventStoreTeardown();
                case Constants.DocumentDb:
                    return DocumentDbFactory.CreateTeardown();
                default:
                    throw new ConfigurationErrorsException($"Unrecognized provider '{provider}' provide a valid provider");
            }
        }
    }
}