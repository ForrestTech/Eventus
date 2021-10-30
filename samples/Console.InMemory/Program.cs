namespace Eventus.Samples.Console.InMemory
{
    using Configuration;
    using Core;
    using Core.Domain;
    using Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    
    //TODO add sql logging (logging IDbConnection
    //TODO add event store logging
    //TODO add cosmosDB support
    //TODO refactor reflection method handling to cache the method info not the method name, may be better not to throw exceptions when failing to add to cache, separate generic helpers from eventus code
    //TODO add details to readme that event constructor parameter names are important (can we auto validate this)
    //TODO Pass in serialization options (sql and event store providers)
    //TODO add support for paging of events for loading non snapshotable events
    //TODO update event store get stream prefix to be part of options
    //TODO review unit test coverage (snapshot options, aggregate config)
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