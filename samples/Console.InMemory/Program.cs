namespace Eventus.Samples.Console.InMemory
{
    using Configuration;
    using Core;
    using Core.Domain;
    using Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    
    //TODO add timing to log decorator on setting
    //TODO add event store logging
    //TODO add cosmos DB logging
    //TODO review unit test coverage (snapshot options, aggregate config)
    //TODO github action builds
    //TODO add details to readme that event constructor parameter names are important (can we auto validate this)
    //TODO add details to readme on how snapshotting works without a read and how that will effect what snapshots you get
    //TODO add details to readme on how snapshotting is not transactional
    //TODO make a note in readme about NewId
    //TODO add sample for event publishing
    //TODO sort out nuget publishing
    //TODO possible change to snapshot strategy if we loaded from snapshot we could store the details in the aggregate for writing or just store if we are in the modules
    //TODO SQL perf improvements single call to DB for a batch of commits
    //TODO SQL schema migration support
    //TODO is it better to pass the aggregate to the event so that version setting is controlled???
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
