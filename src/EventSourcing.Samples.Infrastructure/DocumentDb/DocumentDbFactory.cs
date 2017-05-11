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

        public static async Task<Repository> CreateDocumentDbRepositoryAsync()
        {
            var readRepo = new ReadModelRepository();

            var repository = new Repository(
                await CreateDocumentDbEventProviderAsync().ConfigureAwait(false),
                await CreateDocumentDbSnapshotProviderAsync().ConfigureAwait(false),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));

            return repository;
        }

        public static ITeardown CreateTeardown()
        {
            return new DocumentDbTeardown(Client, DatabaseId);
        }

        public static Task<IEventStorageProvider> CreateDocumentDbEventProviderAsync()
        {
            return Task.FromResult<IEventStorageProvider>(new DocumentDbStorageProvider(Client, DatabaseId));
        }

        public static Task<ISnapshotStorageProvider> CreateDocumentDbSnapshotProviderAsync()
        {
            //todo move snapshot frequency to config
            return Task.FromResult<ISnapshotStorageProvider>(new DocumentDbSnapShotProvider(Client, DatabaseId, 3));
        }

        private static DocumentClient Client => _client ?? (_client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["DocumentDb.Endpoint"]), ConfigurationManager.AppSettings["DocumentDb.AuthKey"], new ConnectionPolicy { EnableEndpointDiscovery = false }));

        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DocumentDb.DatabaseId"];
    }
}