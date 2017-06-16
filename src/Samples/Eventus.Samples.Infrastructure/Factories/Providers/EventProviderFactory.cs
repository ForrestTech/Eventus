using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Eventus.Cleanup;
using Eventus.EventStore;
using Eventus.Logging;
using Eventus.Storage;

namespace Eventus.Samples.Infrastructure.Factories.Providers
{
    public class EventProviderFactory : ProviderFactory
    {
        private static IEventStoreConnection _connection;

        public EventProviderFactory(int value, string name) : base(value, name)
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
            var snapshot = new EventstoreSnapshotStorageProvider(await GetConnectionAsync().ConfigureAwait(false), 3, GetStreamNamePrefix());
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