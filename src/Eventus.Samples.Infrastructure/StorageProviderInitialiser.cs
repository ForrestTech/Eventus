using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Eventus.DocumentDb;
using Eventus.DocumentDb.Config;
using Eventus.Samples.Core.Domain;

namespace Eventus.Samples.Infrastructure
{
    public class StorageProviderInitialiser
    {
        public static async Task InitAsync(object provider)
        {
            var providerToUse = ConfigurationManager.AppSettings["Provider"].ToLowerInvariant();
            switch (providerToUse)
            {
                case Constants.Eventstore:
                    // do nothing
                    break;
                case Constants.DocumentDb:
                    await ((DocumentDbProviderBase)provider).InitAsync(DocumentDbConfig).ConfigureAwait(false);
                    break;
                default:
                    throw new ConfigurationErrorsException($"Unrecognized provider '{providerToUse}' provide a valid provider");
            }
        }

        //todo load from config
        protected static readonly DocumentDbEventStoreConfig DocumentDbConfig = new DocumentDbEventStoreConfig
        {
            AggregateConfig = new List<AggregateConfig>
            {
                new AggregateConfig
                {
                    AggregateType = typeof(BankAccount),
                    OfferThroughput = 400,
                    SnapshotOfferThroughput = 400
                }
            }
        };
    }
}