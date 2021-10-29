namespace Eventus.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;

    public class EventusConfigurator : IEventusConfigurator
    {
        public EventusConfigurator(IServiceCollection collection)
        {
            Collection = collection;
        }
        
        public IServiceCollection Collection { get; }
    }
}