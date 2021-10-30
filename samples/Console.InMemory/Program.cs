namespace Eventus.Samples.Console.InMemory
{
    using Configuration;
    using Core;
    using Core.Domain;
    using Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    
    //TODO add cosmosDB support
    //TODO add event store logging
    //TODO Pass in serialization options (sql and event store providers)
    //TODO add support for paging of events for loading non snapshotable events
    //TODO update event store get stream prefix to be part of options
    //TODO review unit test coverage (snapshot options, aggregate config)
    //TODO possible change to snapshot strategy if we loaded from snapshot we could store the details in the aggregate for writing
    //TODO add details to readme that event constructor parameter names are important (can we auto validate this)
    //TODO add details to readme on how snapshotting works without a read and how that will effect what snapshots you get
    //TODO add details to readme on how snapshotting is not transactional
    //TODO Add timing to log decorators if tracing is enabled 
    //TODO SQL perf improvements single call to DB for a batch of commits
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