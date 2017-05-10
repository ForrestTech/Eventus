using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using EventSourceDemo;
using EventSourceDemo.Domain;
using EventSourceDemo.EventHandlers;
using EventSourceDemo.ReadModel;
using EventSourcing.Cleanup;
using EventSourcing.DocumentDb;
using EventSourcing.DocumentDb.Config;
using EventSourcing.Storage;
using Microsoft.Azure.Documents.Client;

namespace EventSourcing.Samples.Infrastructure
{
    public class DocumentDbFactory
    {
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

        private static DocumentClient _client;

        public static async Task<Repository> CreateDocumentDbRepository()
        {
            var client = DocumentDbClient;

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
            return new DocumentDbTearDown(DocumentDbClient, DatabaseId);
        }

        public static DocumentDbStorageProvider CreateDocumentDbStorageProvider()
        {
            return new DocumentDbStorageProvider(DocumentDbClient, DatabaseId);
        }

        private static async Task InitStorageProvider(DocumentDbProviderBase documentDbStorageProvider)
        {
            //todo move throughput to config
            await documentDbStorageProvider.InitAsync(DocumentDbConfig);
        }

        private static DocumentClient DocumentDbClient => _client ?? (_client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["DocumentDb.Endpoint"]), ConfigurationManager.AppSettings["DocumentDb.AuthKey"], new ConnectionPolicy { EnableEndpointDiscovery = false }));

        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DocumentDb.DatabaseId"];
    }
}