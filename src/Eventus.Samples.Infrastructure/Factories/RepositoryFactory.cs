using System;
using System.Configuration;
using System.Threading.Tasks;
using Eventus.Cleanup;
using Eventus.Logging;
using Eventus.Samples.Core;
using Eventus.Samples.Core.EventHandlers;
using Eventus.Samples.Core.ReadModel;
using Eventus.Samples.Infrastructure.DocumentDb;
using Eventus.SqlServer;
using Eventus.Storage;

namespace Eventus.Samples.Infrastructure.Factories
{
    public class RepositoryFactory
    {
        public static Task<IRepository> CreateAsync(bool initProvider = false, bool addLogging = false)
        {
            var provider = Enumeration.FromDisplayName<StorageProvider>(ConfigurationManager.AppSettings["StorageProvider"].ToLowerInvariant());
            switch (provider)
            {
                case StorageProvider.EventStore:
                    return EventStoreFactory.CreateEventStoreRepositoryAsync(addLogging);
                case StorageProvider.DocumentDb:
                    return DocumentDbFactory.CreateDocumentDbRepositoryAsync(initProvider, addLogging);
                case StorageProvider.SqlServer:
                    break;
                default:
                    throw new ConfigurationErrorsException($"Unrecognized provider '{provider}' provide a valid provider");
            }
        }
    }

    public class StorageProvider : Enumeration
    {
        public static readonly StorageProvider SqlServer = new SqlServerProvider(0, "SqlServerProvider");
        public static readonly StorageProvider DocumentDb = new DocumentDbProvider(0, "SqlServerProvider");
        public static readonly StorageProvider EventStore = new SqlServerProvider(0, "SqlServerProvider");

        protected StorageProvider(int value, string displayName) : base(value, displayName)
        {
        }

        public virtual Task<IRepository> CreateRepositoryAsync()
        {
            throw new InvalidOperationException();
        }

        public virtual Task<ITeardown> CreateTeardownAsync()
        {
            throw new InvalidOperationException();
        }

        public virtual Task<IEventStorageProvider> CreateEventStorageProviderAsync()
        {
            throw new InvalidOperationException();
        }

        public virtual Task<ISnapshotStorageProvider> CreateSnapshotStorageProviderAsync()
        {
            throw new InvalidOperationException();
        }

        private class SqlServerProvider : StorageProvider
        {
            public SqlServerProvider(int value, string displayName) : base(value, displayName)
            {
            }

            public override async Task<IRepository> CreateRepositoryAsync()
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
        }

        private class DocumentDbProvider : StorageProvider
        {
            public DocumentDbProvider(int value, string displayName) : base(value, displayName)
            {
            }
        }
}
}
