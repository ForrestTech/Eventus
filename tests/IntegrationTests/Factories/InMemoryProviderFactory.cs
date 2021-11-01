namespace Eventus.IntegrationTests.Factories
{
    using Configuration;
    using Extensions.DependencyInjection;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Samples.Core.Domain;
    using Storage;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class InMemoryProviderFactory : IProviderFactory
    {
        public string Key
        {
            get
            {
                return "InMemory";
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

        private static ServiceProvider BuildServiceProvider(ITestOutputHelper output)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(new XunitLoggerProvider(output));
            services.AddEventus(options =>
            {
                options.SnapshotFrequency = 10;
                options.AggregateConfigs.Add(new AggregateConfig(typeof(BankAccount))
                {
                    SnapshotFrequency = 3
                });
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        public Task Teardown()
        {
            InMemoryEventStorageProvider.Storage.Clear();
            return Task.CompletedTask;
        }
    }
}