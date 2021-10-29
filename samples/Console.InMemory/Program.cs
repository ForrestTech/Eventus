namespace Eventus.Samples.Console.InMemory
{
    using Core;
    using Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    //TODO pass in assembly to detect aggregates (validate aggregate can apply events)
    //TODO Centralise teardown logic in sample logic
    //TODO provide way to disable snapshots globally and control snapshot frequencey for a given aggregate type
    //TODO add sql logging
    //TODO add details to readme that event constructor parameter names are important (can we auto validate this)
    //TODO add support for paging of events for loading non snapshotable events
    static class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddEventus(options =>
            {
                options.SnapshotOptions.SnapshotFrequency = 5;
            });

            var serviceProvider = services.BuildServiceProvider();

            await SampleLogic.Run(serviceProvider);
        }
    }
}