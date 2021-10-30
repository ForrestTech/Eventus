namespace Eventus.Samples.Console.EventStore
{
    using Core;
    using Eventus.EventStore;
    using Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            //start event store with this docker command: docker run -d --name esdb-node -it -p 2113:2113 -p 1113:1113 eventstore/eventstore:latest --insecure --run-projections=All --enable-external-tcp --enable-atom-pub-over-http
            
            const string connectionString = "ConnectTo=tcp://admin:changeit@localhost:1113;UseSslConnection=false";
            
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddEventus(options =>
            {
                options.SnapshotFrequency = 3;
            }).UseEventStore(connectionString);

            var serviceProvider = services.BuildServiceProvider();
            await SampleLogic.Run(serviceProvider);
        }
    }
}
