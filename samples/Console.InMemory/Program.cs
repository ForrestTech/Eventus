namespace Eventus.Samples.Console.InMemory
{
    using Configuration;
    using Core;
    using Core.Domain;
    using Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    static class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddEventus(options =>
            {
                options.SnapshotFrequency = 10;
                options.AggregateConfigs.Add(new AggregateConfig(typeof(BankAccount))
                {
                    SnapshotFrequency = 3
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            await SampleLogic.Run(serviceProvider);
        }
    }
}
