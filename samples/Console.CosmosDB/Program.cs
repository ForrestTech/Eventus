namespace Console.CosmosDB
{
    using Eventus.Configuration;
    using Eventus.CosmosDB;
    using Eventus.Extensions.DependencyInjection;
    using Eventus.Samples.Core;
    using Eventus.Samples.Core.Cleanup;
    using Eventus.Samples.Core.Domain;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            // docker docker run -p 8081:8081 -p 10251:10251 -p 10252:10252 -p 10253:10253 -p 10254:10254  -m 3g --cpus=2.0 --name=test-linux-emulator -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true -e AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=$ipaddr -it mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
            // download cert and install  curl -k https://localhost:8081/_explorer/emulator.pem > emulatorcert
            var connectionString =
                "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug));
            const string databaseId = "eventus";
            services.AddEventus(typeof(BankAccount), options =>
            {
                options.DiagnosticTimingEnabled = true;
                options.SnapshotFrequency = 10;
                options.AggregateConfigs.Add(new AggregateConfig(typeof(BankAccount))
                {
                    SnapshotFrequency = 3
                });
            }).UseCosmosDB(connectionString, databaseId, cosmosOptions =>
            {
                cosmosOptions.AggregateContainersThroughput = 400;
                cosmosOptions.SnapshotContainersThroughput = 400;
            });

            var cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions {ApplicationName = "Eventus"});
            services.AddSingleton<ITeardown>(new CosmosDBTeardown(cosmosClient, databaseId));

            var serviceProvider = services.BuildServiceProvider();
            await SampleLogic.Run(serviceProvider);
        }
    }
}
