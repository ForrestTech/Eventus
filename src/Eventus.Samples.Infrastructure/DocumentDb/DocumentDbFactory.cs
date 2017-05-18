using System;
using System.Configuration;
using System.Threading.Tasks;
using Eventus.Cleanup;
using Eventus.DocumentDb;
using Eventus.Logging;
using Eventus.Samples.Core;
using Eventus.Samples.Core.EventHandlers;
using Eventus.Samples.Core.ReadModel;
using Eventus.Storage;
using Microsoft.Azure.Documents.Client;

namespace Eventus.Samples.Infrastructure.DocumentDb
{
    public class DocumentDbFactory
    {
        private static DocumentClient _client;

        public static async Task<IRepository> CreateDocumentDbRepositoryAsync(bool addLogging)
        {
            var readRepo = new ReadModelRepository();

            var repository = new Repository(
                await CreateDocumentDbEventProviderAsync(addLogging).ConfigureAwait(false),
                await CreateDocumentDbSnapshotProviderAsync(addLogging).ConfigureAwait(false),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));

            IRepository result;

            if (addLogging)
            {
                result = new RepositoryLoggingDecorator(repository);
            }
            else
            {
                result = repository;
            }

            return result;
        }

        public static ITeardown CreateTeardown()
        {
            return new DocumentDbTeardown(Client, DatabaseId);
        }

        public static Task<IEventStorageProvider> CreateDocumentDbEventProviderAsync(bool addLogging)
        {
            var repo = new DocumentDbStorageProvider(Client, DatabaseId);
            var result = addLogging ?
                (IEventStorageProvider) new EventStorageProviderLoggingDecorator(repo) : 
                repo;
            return Task.FromResult(result);
        }

        public static Task<ISnapshotStorageProvider> CreateDocumentDbSnapshotProviderAsync(bool addLogging)
        {
            //todo move snapshot frequency to config
            var snapshot = new DocumentDbSnapShotProvider(Client, DatabaseId, 3);
            var result = addLogging
                ? (ISnapshotStorageProvider)new SnapshotProviderLoggingDecorator(snapshot)
                : snapshot;

            return Task.FromResult(result);
        }

        private static DocumentClient Client => _client ?? (_client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["DocumentDb.Endpoint"]), ConfigurationManager.AppSettings["DocumentDb.AuthKey"], new ConnectionPolicy { EnableEndpointDiscovery = false }));

        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DocumentDb.DatabaseId"];
    }
}