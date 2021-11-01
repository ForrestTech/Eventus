using Eventus.Configuration;

namespace Eventus.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;

    public class EventusBuilder
    {
        public EventusBuilder(IServiceCollection services, EventusOptions eventusOptions)
        {
            Services = services;
            EventusOptions = eventusOptions;
        }

        public IServiceCollection Services { get; }
        
        public EventusOptions EventusOptions { get; }
    }
}
