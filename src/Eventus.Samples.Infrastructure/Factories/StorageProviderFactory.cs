using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Eventus.Cleanup;
using Eventus.DocumentDb;
using Eventus.DocumentDb.Config;
using Eventus.EventStore;
using Eventus.Logging;
using Eventus.Samples.Core;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.Core.EventHandlers;
using Eventus.Samples.Core.ReadModel;
using Eventus.SqlServer;
using Eventus.Storage;
using Microsoft.Azure.Documents.Client;

namespace Eventus.Samples.Infrastructure.Factories
{
    public abstract class StorageProviderFactory 
    {
        public static readonly StorageProviderFactory SqlServer = new SqlServerProviderFactory(0, "SqlServer");
        public static readonly StorageProviderFactory DocumentDb = new DocumentDbProviderFactory(1, "DocumentDb");
        public static readonly StorageProviderFactory EventStore = new EventStorageProviderFactory(2, "EventStore");

        public string Name { get; }
        public int Value { get; }

        protected StorageProviderFactory(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public static IEnumerable<StorageProviderFactory> List()
        {
            return new[] { SqlServer, DocumentDb, EventStore };
        }

        public static StorageProviderFactory FromString(string roleString)
        {
            return List().Single(r => string.Equals(r.Name, roleString, StringComparison.OrdinalIgnoreCase));
        }

        public virtual async Task<IRepository> CreateRepositoryAsync()
        {
            var readRepo = new ReadModelRepository();

            var repo = new RepositoryLoggingDecorator(
                new Repository(
                    await CreateEventStorageProviderAsync().ConfigureAwait(false),
                    await CreateSnapshotStorageProviderAsync().ConfigureAwait(false),
                    new DemoPublisher(
                        new DepositEventHandler(readRepo),
                        new WithdrawalEventHandler(readRepo))));

            return repo;
        }

        public abstract Task<ITeardown> CreateTeardownAsync();

        public abstract Task<IEventStorageProvider> CreateEventStorageProviderAsync();

        public abstract Task<ISnapshotStorageProvider> CreateSnapshotStorageProviderAsync();

        public abstract Task InitAsync();

        private class SqlServerProviderFactory : StorageProviderFactory
        {
            public SqlServerProviderFactory(int value, string displayName) : base(value, displayName)
            {
            }

            public override Task<ITeardown> CreateTeardownAsync()
            {
                return Task.FromResult<ITeardown>(new SqlServerTeardown());
            }

            public override Task<IEventStorageProvider> CreateEventStorageProviderAsync()
            {
                return Task.FromResult<IEventStorageProvider>(new SqlServerEventStorageProvider(ConfigurationManager
                    .ConnectionStrings["ConnectionString"].ToString()));
            }

            public override Task<ISnapshotStorageProvider> CreateSnapshotStorageProviderAsync()
            {
                return Task.FromResult<ISnapshotStorageProvider>(new SqlServerSnapshotStorageProvider(
                    ConfigurationManager.ConnectionStrings["ConnectionString"].ToString(), 3));
            }

            public override Task InitAsync()
            {
                return Task.CompletedTask;
            }
        }

        private class DocumentDbProviderFactory : StorageProviderFactory
        {
            private static DocumentClient _client;

            public DocumentDbProviderFactory(int value, string displayName) : base(value, displayName)
            {
            }

            public override Task<ITeardown> CreateTeardownAsync()
            {
                return Task.FromResult<ITeardown>(new DocumentDbTeardown(Client, DatabaseId));
            }

            public override Task<IEventStorageProvider> CreateEventStorageProviderAsync()
            {
                var provider = new DocumentDbStorageProvider(Client, DatabaseId);
                return Task.FromResult<IEventStorageProvider>(new EventStorageProviderLoggingDecorator(provider));
            }

            public override Task<ISnapshotStorageProvider> CreateSnapshotStorageProviderAsync()
            {
                //todo move frequency to config
                var provider = new DocumentDbSnapShotProvider(Client, DatabaseId, 3);
                return Task.FromResult<ISnapshotStorageProvider>(new SnapshotProviderLoggingDecorator(provider));
            }

            public override Task InitAsync()
            {
                var init = new DocumentDbInitialiser(Client, DatabaseId);
                return init.InitAsync(DocumentDbConfig);
            }

            private static readonly DocumentDbEventStoreConfig DocumentDbConfig = new DocumentDbEventStoreConfig
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

            private static DocumentClient Client => _client ?? (_client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["DocumentDb.Endpoint"]), ConfigurationManager.AppSettings["DocumentDb.AuthKey"], new ConnectionPolicy { EnableEndpointDiscovery = false }));

            private static readonly string DatabaseId = ConfigurationManager.AppSettings["DocumentDb.DatabaseId"];
        }

        private class EventStorageProviderFactory : StorageProviderFactory
        {
            private static IEventStoreConnection _connection;

            public EventStorageProviderFactory(int value, string name) : base(value, name)
            {
            }

            public override Task<ITeardown> CreateTeardownAsync()
            {
                return Task.FromResult<ITeardown>(new EventStoreTeardown());
            }

            public override async Task<IEventStorageProvider> CreateEventStorageProviderAsync()
            {
                var eventStore = new EventstoreStorageProvider(await GetConnectionAsync().ConfigureAwait(false), GetStreamNamePrefix());
                return new EventStorageProviderLoggingDecorator(eventStore);
            }

            public override async Task<ISnapshotStorageProvider> CreateSnapshotStorageProviderAsync()
            {
                var snapshot = new EventstoreSnapshotStorageProvider(await GetConnectionAsync().ConfigureAwait(false), GetStreamNamePrefix(), 3);
                return new SnapshotProviderLoggingDecorator(snapshot);
            }

            public override Task InitAsync()
            {
                return Task.CompletedTask;
            }

            private static async Task<IEventStoreConnection> GetConnectionAsync()
            {
                if (_connection != null)
                    return _connection;

                //todo connection setting to config
                _connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
                await _connection.ConnectAsync()
                    .ConfigureAwait(false);

                return _connection;
            }

            private static Func<string> GetStreamNamePrefix()
            {
                return () => !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EventStore.StreamPrefix"]) ? ConfigurationManager.AppSettings["EventStore.StreamPrefix"] : "Demo-";
            }
        }
    }
}