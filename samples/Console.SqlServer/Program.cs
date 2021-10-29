namespace Console.SqlServer
{
    using Eventus.Extensions.DependencyInjection;
    using Eventus.Samples.Core;
    using Eventus.SqlServer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            const string connectionString = "Server=127.0.0.1,1433;Database=Eventus;User Id=sa;Password=yourStrong(!)Password;";
            
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddEventus(options =>
            {
                options.SnapshotOptions.SnapshotFrequency = 3;
            }).UseSqlServer(connectionString);

            var tear = new SqlServerTeardown(connectionString);
            await tear.TearDownAsync();
            
            var serviceProvider = services.BuildServiceProvider();
            await SampleLogic.Run(serviceProvider);
        }
    }
}
