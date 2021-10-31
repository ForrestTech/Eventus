namespace Eventus.Extensions.DependencyInjection
{
    using Configuration;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class ServiceCollectionExtensions
    {
        public static EventusBuilder AddEventus(this IServiceCollection services,
            Type aggregateType,
            Action<EventusOptions>? configureOptions = null)
        {
            return services.AddEventus(aggregateType.GetTypeInfo().Assembly, configureOptions);
        }
        
        public static EventusBuilder AddEventus(this IServiceCollection services,
            Assembly aggregateAssembly,
            Action<EventusOptions>? configureOptions = null)
        {
            return services.AddEventus(configureOptions, aggregateAssembly);
        }
        
        public static EventusBuilder AddEventus(this IServiceCollection services,
            params Assembly[] assemblies)
        {
            return services.AddEventus(assemblies, null);
        }
        
        public static EventusBuilder AddEventus(this IServiceCollection services,
            Action<EventusOptions>? configureOptions = null,
            params Assembly[] assemblies)
        {
            return services.AddEventus(assemblies, configureOptions);
        }
        
        public static EventusBuilder AddEventus(this IServiceCollection services,
            params Type[] aggregateAssemblyTypes)
        {
            return services.AddEventus(null, aggregateAssemblyTypes);
        }
        
        public static EventusBuilder AddEventus(this IServiceCollection services,
            Action<EventusOptions>? configureOptions = null,
            params Type[] aggregateAssemblyTypes)
        {
            return services.AddEventus(aggregateAssemblyTypes.Select(t => t.GetTypeInfo().Assembly), configureOptions);
        }
        
        public static EventusBuilder AddEventus(this IServiceCollection services,
            Action<EventusOptions>? configureOptions = null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            
            return services.AddEventus(assemblies, configureOptions);
        }
        
            
        public static EventusBuilder AddEventus(this IServiceCollection services,
            IEnumerable<Assembly> aggregateAssemblies,
            Action<EventusOptions>? configureOptions = null)
        {
            var options = new EventusOptions();
            configureOptions?.Invoke(options);

            services.AddSingleton(options);
            
            services.AddTransient<ISnapshotCalculator, SnapshotCalculator>();

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

            var aggregateAssemblyList = aggregateAssemblies.ToList();
            
            AggregateCache.AggregateAssemblies = aggregateAssemblyList;
            
            AggregateValidation.AssertThatAggregatesSupportAllEvents(aggregateAssemblyList);

            return new EventusBuilder(services);
        }
    }
}