using System;
using System.Net;
using EventSourceDemo.Commands;
using EventSourceDemo.Domain;
using EventSourceDemo.EventHandlers;
using EventSourceDemo.Handlers;
using EventSourceDemo.ReadModel;
using EventSourcing.EventSource;
using EventSourcing.Repository;
using EventStore.ClientAPI;

namespace EventSourceDemo
{
    class Program
    {
        //todo
        //create validators
        //direct event store subscription
        //sql implementation of event store
        //martin implementation of event store
        static void Main(string[] args)
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            connection.ConnectAsync();

            var accountId = Guid.NewGuid();
            var readRepo = new ReadModelRepository();

            var repo = new Repository(
                new EventstoreStorageProvider(connection, GetStreamNamePrefix()),
                new EventstoreSnapshotStorageProvider(connection, GetStreamNamePrefix(), 3),
                new DemoPublisher(
                    new HandleDeposit(readRepo),
                    new HandleWithdrawal(readRepo)));

            var handler = new BankAccountHandlers(repo);

            handler.HandleAsync(new CreateAccountCommand(Guid.NewGuid(), accountId, "Joe Bloggs")).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 35)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 25)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 5)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();

            var fromStore = repo.GetByIdAsync<BankAccount>(accountId).Result;

            Console.ReadLine();
        }

        private static Func<string> GetStreamNamePrefix()
        {
            return () => "Demo-";
        }
    }
}