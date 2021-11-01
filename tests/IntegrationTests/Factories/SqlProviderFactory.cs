namespace Eventus.IntegrationTests.Factories
{
    using Configuration;
    using Extensions.DependencyInjection;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Samples.Core.Domain;
    using SqlServer;
    using Storage;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class SqlProviderFactory : IProviderFactory
    {
        private const string ConnectionString = "Server=127.0.0.1,1433;Database=Eventus;User Id=sa;Password=yourStrong(!)Password;";

        public string Key
        {
            get
            {
                return "SQL";
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
            services.AddEventus(typeof(BankAccount), options =>
            {
                options.SnapshotFrequency = 10;
                options.AggregateConfigs.Add(new AggregateConfig(typeof(BankAccount))
                {
                    SnapshotFrequency = 3
                });
            }).UseSqlServer(ConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        public async Task Teardown()
        {
            var teardown = new SqlServerTeardown(ConnectionString);
            await teardown.TearDownAsync();
        }
    }
}