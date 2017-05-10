using System;
using System.Configuration;
using System.Net;
using EventSourceDemo;
using EventSourceDemo.EventHandlers;
using EventSourceDemo.ReadModel;
using EventSourcing.EventStore;
using EventSourcing.Storage;
using EventStore.ClientAPI;

namespace EventSourcing.Samples.Infrastructure
{
    public class EventStoreFactory
    {
        public static Repository CreateEventStoreRepository()
        {
            //todo move to configuration
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            connection.ConnectAsync();

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