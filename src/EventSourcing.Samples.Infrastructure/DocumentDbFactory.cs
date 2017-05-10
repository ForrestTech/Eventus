using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using EventSourcing.Cleanup;
using EventSourcing.DocumentDb;
using EventSourcing.DocumentDb.Config;
using EventSourcing.Samples.Core;
using EventSourcing.Samples.Core.Domain;
using EventSourcing.Samples.Core.EventHandlers;
using EventSourcing.Samples.Core.ReadModel;
using EventSourcing.Storage;
using Microsoft.Azure.Documents.Client;

namespace EventSourcing.Samples.Infrastructure
{
    public class DocumentDbFactory
    {
        private static DocumentClient _client;

        public static readonly DocumentDbEventStoreConfig DocumentDbConfig = new DocumentDbEventStoreConfig
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

        public static async Task<Repository> CreateDocumentDbRepository()
        {
            var client = Client;

            var documentDbStorageProvider = CreateDocumentDbStorageProvider();
            
            await InitStorageProvider(documentDbStorageProvider);

            var readRepo = new ReadModelRepository();

            return new Repository(
                documentDbStorageProvider,
                new DocumentDbSnapShotProvider(client, DatabaseId, 3),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));
        }

        public static ITeardown CreateTeardown()
        {
            return new DocumentDbTearDown(Client, DatabaseId);
        }

        public static DocumentDbStorageProvider CreateDocumentDbStorageProvider()
        {
            return new DocumentDbStorageProvider(Client, DatabaseId);
        }

        private static async Task InitStorageProvider(DocumentDbProviderBase documentDbStorageProvider)
        {
            //todo move throughput to config
            await documentDbStorageProvider.InitAsync(DocumentDbConfig);
        }

        public static DocumentDbSnapShotProvider CreateDocumentDbSnapShotProvider()
        {
            //todo move snapshot frequency to config
            return new DocumentDbSnapShotProvider(Client, DatabaseId, 3);
        }

        private static DocumentClient Client => _client ?? (_client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["DocumentDb.Endpoint"]), ConfigurationManager.AppSettings["DocumentDb.AuthKey"], new ConnectionPolicy { EnableEndpointDiscovery = false }));

        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DocumentDb.DatabaseId"];
    }
}