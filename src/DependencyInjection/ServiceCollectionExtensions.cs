namespace Eventus.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;
    using Storage;
    using System;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventus(this IServiceCollection services,
            Action<EventusConfigurator>? configure = null)
        {
            services.AddTransient<IRepository, Repository>();

            var eventusConfiguration = new EventusConfigurator(services);

            configure?.Invoke(eventusConfiguration);

            //TODO validate at this stage that storage has been provided

            return services;
        }
    }
}