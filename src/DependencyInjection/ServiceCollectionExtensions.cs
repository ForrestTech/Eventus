namespace Eventus.Extensions.DependencyInjection
{
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Storage;
    using System;

    public static class ServiceCollectionExtensions
    {
        public static EventusBuilder AddEventus(this IServiceCollection services,
            Action<EventusOptions>? configureOptions = null)
        {
            var options = new EventusOptions();
            configureOptions?.Invoke(options);

            services.AddSingleton(options);

            services.AddTransient<InMemoryEventStorageProvider>();
            services.AddTransient<IEventStorageProvider>(x =>
            {
                var logger = x.GetService<ILogger<EventStorageProviderLoggingDecorator>>() ?? throw new InvalidOperationException();
                var toDecorate = x.GetService<InMemoryEventStorageProvider>() ?? throw new InvalidOperationException();
                var decorated = new EventStorageProviderLoggingDecorator(
                    toDecorate,
                    logger);
                return decorated;
            });
            
            services.AddTransient<InMemorySnapshotStorageProvider>();
            services.AddTransient<ISnapshotStorageProvider>(x =>
            {
                var logger = x.GetService<ILogger<SnapshotProviderLoggingDecorator>>() ?? throw new InvalidOperationException();
                var toDecorate = x.GetService<InMemorySnapshotStorageProvider>() ?? throw new InvalidOperationException();
                var decorated = new SnapshotProviderLoggingDecorator(
                    toDecorate,
                    logger);
                return decorated;
            });
            
            services.AddTransient<Repository>();
            services.AddTransient<IRepository>(x =>
            {
                var logger = x.GetService<ILogger<RepositoryLoggingDecorator>>() ?? throw new InvalidOperationException();
                var toDecorate = x.GetService<Repository>() ?? throw new InvalidOperationException();
                var decorated = new RepositoryLoggingDecorator(
                    logger,
                    toDecorate);
                return decorated;
            });

            return new EventusBuilder(services);
        }
    }
}