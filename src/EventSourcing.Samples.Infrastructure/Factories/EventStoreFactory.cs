using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using EventSourcing.EventStore;
using EventSourcing.Logging;
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

        public static async Task<IRepository> CreateEventStoreRepositoryAsync(bool addLogging = false)
        {
            var readRepo = new ReadModelRepository();

            var repo = new Repository(
                await CreateEventStoreEventStorageProviderAsync(addLogging).ConfigureAwait(false),
                await CreateSnapshotStorageProviderAsync(addLogging).ConfigureAwait(false),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));

            IRepository result;

            if (addLogging)
            {
                result = new RepositoryLoggingDecorator(repo);
            }
            else
            {
                result = repo;
            } 

            return result;
        }

        public static async Task<IEventStorageProvider> CreateEventStoreEventStorageProviderAsync(bool addLogging)
        {
            var eventStore = new EventstoreStorageProvider(await GetConnectionAsync().ConfigureAwait(false), GetStreamNamePrefix());

            IEventStorageProvider result;
            if (addLogging)
            {
                result = new EventStorageProviderLoggingDecorator(eventStore);
            }
            else
            {
                result = eventStore;
            }

            return result;
        }

        public static async Task<ISnapshotStorageProvider> CreateSnapshotStorageProviderAsync(bool addLogging)
        {
            var snapshot = new EventstoreSnapshotStorageProvider(await GetConnectionAsync().ConfigureAwait(false), GetStreamNamePrefix(), 3);

            ISnapshotStorageProvider result;

            if (addLogging)
            {
                result = new SnapshotProviderLoggingDecorator(snapshot);
            }
            else
            {
                result = snapshot;
            }

            return result;
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