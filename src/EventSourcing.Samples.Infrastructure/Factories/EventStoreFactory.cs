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
        public static async Task<Repository> CreateEventStoreRepository()
        {
            //todo move to configuration
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            await connection.ConnectAsync();

            var readRepo = new ReadModelRepository();

            return new Repository(
                new EventstoreStorageProvider(connection, GetStreamNamePrefix()),
                new EventstoreSnapshotStorageProvider(connection, GetStreamNamePrefix(), 3),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));
        }

        private static Func<string> GetStreamNamePrefix()
        {
            return () => !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["EventStore.StreamPrefix"]) ? ConfigurationManager.AppSettings["EventStore.StreamPrefix"] : "Demo-";
        }
    }
}