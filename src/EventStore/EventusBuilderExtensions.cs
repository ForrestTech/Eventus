namespace Eventus.EventStore
{
    using Extensions.DependencyInjection;
    using global::EventStore.ClientAPI;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Storage;
    using System;

    public static class EventusBuilderExtensions
    {
        public static EventusBuilder UseEventStore(this EventusBuilder builder, string connectionString,
            Action<EventusEventStoreOptions>? optionsConfig = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));
            }

            var options = new EventusEventStoreOptions(connectionString);

            optionsConfig?.Invoke(options);

            builder.Services.AddSingleton(options);

            builder.Services.AddSingleton<EventStoreStorageProvider>();
            builder.Services.AddSingleton<IEventStorageProvider>(x =>
            {
                var logger = x.GetService<ILogger<EventStorageProviderLoggingDecorator>>() ??
                             throw new InvalidOperationException();
                var toDecorate = x.GetService<EventStoreStorageProvider>() ?? throw new InvalidOperationException();
                var decorated = new EventStorageProviderLoggingDecorator(
                    toDecorate,
                    logger);
                return decorated;
            });

            builder.Services.AddSingleton<EventStoreSnapshotStorageProvider>();
            builder.Services.AddSingleton<ISnapshotStorageProvider>(x =>
            {
                var logger = x.GetService<ILogger<SnapshotProviderLoggingDecorator>>() ??
                             throw new InvalidOperationException();
                var toDecorate = x.GetService<EventStoreSnapshotStorageProvider>() ??
                                 throw new InvalidOperationException();
                var decorated = new SnapshotProviderLoggingDecorator(
                    toDecorate,
                    logger);
                return decorated;
            });
            
            var connection = EventStoreConnection.Create(connectionString);
            
            connection.ConnectAsync().GetAwaiter().GetResult();

            builder.Services.AddSingleton(connection);

            return builder;
        }
    }
}