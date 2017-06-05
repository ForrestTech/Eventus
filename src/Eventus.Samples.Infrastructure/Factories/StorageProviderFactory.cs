using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Eventus.Cleanup;
using Eventus.Logging;
using Eventus.Samples.Core;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.Core.EventHandlers;
using Eventus.Samples.Core.ReadModel;
using Eventus.Samples.Infrastructure.Config;
using Eventus.Storage;
using AggregateConfig = Eventus.Samples.Infrastructure.Config.AggregateConfig;

namespace Eventus.Samples.Infrastructure.Factories
{
    public abstract partial class StorageProviderFactory
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
    }
}