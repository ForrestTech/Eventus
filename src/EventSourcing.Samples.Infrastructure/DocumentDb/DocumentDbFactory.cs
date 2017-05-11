using System;
using System.Configuration;
using System.Threading.Tasks;
using EventSourcing.Cleanup;
using EventSourcing.DocumentDb;
using EventSourcing.Samples.Core;
using EventSourcing.Samples.Core.EventHandlers;
using EventSourcing.Samples.Core.ReadModel;
using EventSourcing.Storage;
using Microsoft.Azure.Documents.Client;

namespace EventSourcing.Samples.Infrastructure.DocumentDb
{
    public class DocumentDbFactory
    {
        private static DocumentClient _client;

        public static Task<Repository> CreateDocumentDbRepository()
        {
            var client = Client;

            var documentDbStorageProvider = CreateDocumentDbStorageProvider();
            var readRepo = new ReadModelRepository();

            var repository = new Repository(
                documentDbStorageProvider,
                new DocumentDbSnapShotProvider(client, DatabaseId, 3),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));

            return Task.FromResult(repository);
        }

        public static ITeardown CreateTeardown()
        {
            return new DocumentDbTearDown(Client, DatabaseId);
        }

        public static IEventStorageProvider CreateDocumentDbStorageProvider()
        {
            return new DocumentDbStorageProvider(Client, DatabaseId);
        }

        public static ISnapshotStorageProvider CreateDocumentDbSnapShotProvider()
        {
            //todo move snapshot frequency to config
            return new DocumentDbSnapShotProvider(Client, DatabaseId, 3);
        }

        private static DocumentClient Client => _client ?? (_client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["DocumentDb.Endpoint"]), ConfigurationManager.AppSettings["DocumentDb.AuthKey"], new ConnectionPolicy { EnableEndpointDiscovery = false }));

        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DocumentDb.DatabaseId"];
    }
}