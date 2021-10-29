namespace Eventus.InMemory
{
    using Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Storage;

    public static class EventusConfigurationExtensions
    {
        public static void UseInMemoryStorage(this IEventusConfigurator configurator)
        {
            //TDDO configuration logging 
            configurator.Collection.AddTransient<IEventStorageProvider, InMemoryEventStorageProvider>();
            configurator.Collection.AddTransient<ISnapshotStorageProvider, InMemorySnapshotStorageProvider>();
        }
    }
}
