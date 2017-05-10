using System;
using System.Collections.Generic;
using System.Configuration;
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
        private static DocumentClient _client;

        public static Repository CreateDocumentDbRepository()
        {
            var client = DocumentDbClient();

            var documentDbStorageProvider = new DocumentDbStorageProvider(client, DatabaseId);
            documentDbStorageProvider.InitAsync(new DocumentDbEventStoreConfig
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
            }).Wait();

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
            return new DocumentDbTearDown(DocumentDbClient(), DatabaseId);
        }

        private static DocumentClient DocumentDbClient()
        {
            return _client ?? 
                   (_client = new DocumentClient(
                       new Uri(ConfigurationManager.AppSettings["DocumentDb.Endpoint"]),
                       ConfigurationManager.AppSettings["DocumentDb.AuthKey"],
                       new ConnectionPolicy { EnableEndpointDiscovery = false }));
        }

        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DocumentDb.DatabaseId"];
    }
}