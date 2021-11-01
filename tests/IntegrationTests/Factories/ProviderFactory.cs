namespace Eventus.IntegrationTests.Factories
{
    using Storage;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public static class ProviderFactory
    {
        private static readonly List<IProviderFactory> ProviderFactories = new()
        {
            new CosmosDBProviderFactory(),
            new InMemoryProviderFactory(),
            new SqlProviderFactory(),
            new EventStoreProviderFactory()
        };

        public static readonly List<string> Providers = ProviderFactories.Select(x => x.Key).ToList();

        public static IEventStorageProvider GetStorageProvider(string providerKey, ITestOutputHelper output)
        {
            return ProviderFactories.Single(x => x.Key == providerKey).GetStorageProvider(output);
        }
        
        public static ISnapshotStorageProvider GetSnapshotProvider(string providerKey, ITestOutputHelper output)
        {
            return ProviderFactories.Single(x => x.Key == providerKey).GetSnapshotProvider(output);
        }

        public static IRepository CreateRepository(string providerKey, ITestOutputHelper output)
        {
            return ProviderFactories.Single(x => x.Key == providerKey).CreateRepository(output);
        }

        public static Task Teardown()
        {
            foreach (var factory in ProviderFactories)
            {
                factory.Teardown().Wait();
            }
            
            return Task.CompletedTask;;
        }
    }
}
