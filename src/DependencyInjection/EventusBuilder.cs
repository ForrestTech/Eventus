namespace Eventus.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;

    public class EventusBuilder
    {
        public EventusBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}