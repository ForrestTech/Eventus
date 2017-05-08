using System;
using System.Configuration;
using System.Net;
using EventSourceDemo.Commands;
using EventSourceDemo.Domain;
using EventSourceDemo.EventHandlers;
using EventSourceDemo.Handlers;
using EventSourceDemo.ReadModel;
using EventSourcing.EventStore;
using EventSourcing.Repository;
using EventStore.ClientAPI;
using Microsoft.Azure.Documents.Client;
using EventSourcing.DocumentDb;

namespace EventSourceDemo
{
    class Program
    {        
        static void Main(string[] args)
        {
            var accountId = Guid.NewGuid();
            var readRepo = new ReadModelRepository();

            //var repo = EventStoreRepository(readRepo);
            var repo = DocumentDbRepository(readRepo);

            var handler = new BankAccountCommandHandlers(repo);

            handler.HandleAsync(new CreateAccountCommand(Guid.NewGuid(), accountId, "Joe Bloggs")).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 35)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 25)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 5)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();

            var fromStore = repo.GetByIdAsync<BankAccount>(accountId).Result;

            Console.ReadLine();
        }

        private static Repository EventStoreRepository(ReadModelRepository readRepo)
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            connection.ConnectAsync();


            return new Repository(
                new EventstoreStorageProvider(connection, GetStreamNamePrefix()),
                new EventstoreSnapshotStorageProvider(connection, GetStreamNamePrefix(), 3),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));
        }

        private static Repository DocumentDbRepository(ReadModelRepository readRepo)
        {
            var client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"], new ConnectionPolicy { EnableEndpointDiscovery = false });

            return new Repository(
                new DocumentDbStorageProvider(client, "test1"), 
                null,//new DocumentDbSnapShotProvider(client), 
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));
        }

        private static Func<string> GetStreamNamePrefix()
        {
            return () => "Demo-";
        }
    }
}