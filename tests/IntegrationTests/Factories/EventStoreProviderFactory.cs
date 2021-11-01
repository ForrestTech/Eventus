namespace Eventus.IntegrationTests.Factories
{
    using EventStore;
    using Extensions.DependencyInjection;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Storage;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class EventStoreProviderFactory : IProviderFactory
    {
        const string ConnectionString = "ConnectTo=tcp://admin:changeit@localhost:1113;UseSslConnection=false";

        public string Key
        {
            get
            {
                return "EventStore";
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

        private static ServiceProvider BuildServiceProvider(ITestOutputHelper output)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(new XunitLoggerProvider(output));
            services.AddEventus(options =>
            {
                options.SnapshotFrequency = 3;
            }).UseEventStore(ConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        public Task Teardown()
        {
            //nothing to do here
            return Task.CompletedTask;
        }
    }
}
