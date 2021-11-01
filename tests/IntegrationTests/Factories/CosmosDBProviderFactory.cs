namespace Eventus.IntegrationTests.Factories
{
    using Configuration;
    using CosmosDB;
    using Extensions.DependencyInjection;
    using Logging;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.DependencyInjection;
    using Samples.Core.Domain;
    using Storage;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class CosmosDBProviderFactory : IProviderFactory
    {
        private const string ConnectionString =
            "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private const string DatabaseId = "eventus";

        public string Key
        {
            get
            {
                return "CosmosDB";
            }
        }

        public IEventStorageProvider GetStorageProvider(ITestOutputHelper output)
        {
            var serviceProvider = BuildServiceProvider(output);

            return serviceProvider.GetService<IEventStorageProvider>();
        }

        public ISnapshotStorageProvider GetSnapshotProvider(ITestOutputHelper output)
        {
            var serviceProvider = BuildServiceProvider(output);

            return serviceProvider.GetService<ISnapshotStorageProvider>();
        }

        public IRepository CreateRepository(ITestOutputHelper output)
        {
            var serviceProvider = BuildServiceProvider(output);

            return serviceProvider.GetService<IRepository>();
        }

        private ServiceProvider BuildServiceProvider(ITestOutputHelper output)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(new XunitLoggerProvider(output));
            services.AddEventus(typeof(BankAccount), options =>
            {
                options.SnapshotFrequency = 10;
                options.AggregateConfigs.Add(new AggregateConfig(typeof(BankAccount)) {SnapshotFrequency = 3});
            }).UseCosmosDB(ConnectionString, DatabaseId, cosmosOptions =>
            {
                cosmosOptions.AggregateContainersThroughput = 400;
                cosmosOptions.SnapshotContainersThroughput = 400;
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        public async Task Teardown()
        {
            var cosmosClient =
                new CosmosClient(ConnectionString, new CosmosClientOptions {ApplicationName = "Eventus"});
            var teardown = new CosmosDBTeardown(cosmosClient, DatabaseId);
            await teardown.TearDownAsync();
        }
    }
}
