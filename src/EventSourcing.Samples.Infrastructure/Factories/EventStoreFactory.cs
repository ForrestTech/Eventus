using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using EventSourcing.EventStore;
using EventSourcing.Samples.Core;
using EventSourcing.Samples.Core.EventHandlers;
using EventSourcing.Samples.Core.ReadModel;
using EventSourcing.Storage;
using EventStore.ClientAPI;

namespace EventSourcing.Samples.Infrastructure.Factories
{
    public class EventStoreFactory
    {
        private static IEventStoreConnection _connection;

        public static async Task<Repository> CreateEventStoreRepositoryAsync()
        {
            var readRepo = new ReadModelRepository();

            return new Repository(
                await CreateEventStoreEventStorageProviderAsync().ConfigureAwait(false),
                await CreateSnapshotStorageProviderAsync().ConfigureAwait(false),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));
        }

        public static async Task<IEventStorageProvider> CreateEventStoreEventStorageProviderAsync()
        {
            return new EventstoreStorageProvider(await GetConnectionAsync().ConfigureAwait(false), GetStreamNamePrefix());
        }

        public static async Task<ISnapshotStorageProvider> CreateSnapshotStorageProviderAsync()
        {
            return new EventstoreSnapshotStorageProvider(await GetConnectionAsync().ConfigureAwait(false), GetStreamNamePrefix(), 3);
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