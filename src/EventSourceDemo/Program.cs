using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
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
using EventSourcing.DocumentDb.Config;

namespace EventSourceDemo
{
    class Program
    {
        private const string DatabaseId = "Test";
        private static DocumentClient _client;


        static void Main(string[] args)
        {
            var cleaner = Cleaner();
            cleaner.TearDownAsync().Wait();

            var accountId = Guid.NewGuid();
            var readRepo = new ReadModelRepository();

            var repo = Repository(readRepo);

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

        private static Repository Repository(IReadModelRepository readRepo)
        {
            //return EventStoreRepository(readRepo);
            return DocumentDbRepository(readRepo);
        }

        private static ITeardown Cleaner()
        {
            return new DocumentDbTearDown(DocumentDbClient(), DatabaseId);
        }

        private static Repository EventStoreRepository(IReadModelRepository readRepo)
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

        private static Repository DocumentDbRepository(IReadModelRepository readRepo)
        {
            var client = DocumentDbClient();

            //todo change to order docs by versions
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

            return new Repository(
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

    public interface ITeardown
    {
        Task TearDownAsync();
    }

    public class DocumentDbTearDown : ITeardown
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;

        public DocumentDbTearDown(DocumentClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        public async Task TearDownAsync()
        {
            await _client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
        }
    }
}