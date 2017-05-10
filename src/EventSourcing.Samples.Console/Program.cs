using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using EventSourceDemo;
using EventSourceDemo.Commands;
using EventSourceDemo.Domain;
using EventSourceDemo.EventHandlers;
using EventSourceDemo.Handlers;
using EventSourceDemo.ReadModel;
using EventSourcing.Cleanup;
using EventSourcing.DocumentDb;
using EventSourcing.DocumentDb.Config;
using EventSourcing.EventStore;
using EventStore.ClientAPI;
using Microsoft.Azure.Documents.Client;

using static System.Console;

namespace EventSourcing.Samples.Console
{
    class Program
    {
        private const string DatabaseId = "Test";
        private static DocumentClient _client;


        static void Main(string[] args)
        {
            WriteLine("Event sourcing sample");

            WriteLine("Tearing down provider");

            var cleaner = Cleaner();
            cleaner.TearDownAsync().Wait();

            WriteLine("Provider torn down");

            var accountId = Guid.NewGuid();
            var readRepo = new ReadModelRepository();

            var repo = Repository(readRepo);

            var handler = new BankAccountCommandHandlers(repo);

            WriteLine("Running set of commands");

            handler.HandleAsync(new CreateAccountCommand(Guid.NewGuid(), accountId, "Joe Bloggs")).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 35)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 25)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 5)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();

            WriteLine("Commands Run");

            WriteLine("Get aggregate from store");

            var fromStore = repo.GetByIdAsync<BankAccount>(accountId).Result;

            WriteLine($"Bank account ID: {fromStore.Id}");
            WriteLine($"Balance: {fromStore.CurrentBalance}");
            WriteLine($"Last commited version: {fromStore.LastCommittedVersion}");
            WriteLine($"Transaction Count: {fromStore.Transactions.Count}");

            WriteLine("Event sourcing sample ran");

            ReadLine();
        }

        private static Repository.Repository Repository(IReadModelRepository readRepo)
        {
            //return EventStoreRepository(readRepo);
            return DocumentDbRepository(readRepo);
        }

        private static ITeardown Cleaner()
        {
            return new DocumentDbTearDown(DocumentDbClient(), DatabaseId);
        }

        private static Repository.Repository EventStoreRepository(IReadModelRepository readRepo)
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            connection.ConnectAsync();


            return new Repository.Repository(
                new EventstoreStorageProvider(connection, GetStreamNamePrefix()),
                new EventstoreSnapshotStorageProvider(connection, GetStreamNamePrefix(), 3),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));
        }

        private static Repository.Repository DocumentDbRepository(IReadModelRepository readRepo)
        {
            var client = DocumentDbClient();

            var documentDbStorageProvider = new DocumentDbStorageProvider(client, DatabaseId);
            documentDbStorageProvider.InitAsync(new DocumentDbEventStoreConfig
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
            }).Wait();

            return new Repository.Repository(
                documentDbStorageProvider,
                new DocumentDbSnapShotProvider(client, DatabaseId, 3),
                new DemoPublisher(
                    new DepositEventHandler(readRepo),
                    new WithdrawalEventHandler(readRepo)));
        }

        private static DocumentClient DocumentDbClient()
        {
            return _client ?? (_client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"], new ConnectionPolicy { EnableEndpointDiscovery = false }));
        }

        private static Func<string> GetStreamNamePrefix()
        {
            return () => "Demo-";
        }
    }
}
