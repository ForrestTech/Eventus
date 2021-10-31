namespace Eventus.SqlServer
{
    using Configuration;
    using Extensions.DependencyInjection;
    using Logging;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Storage;
    using System;

    public static class EventusBuilderExtensions
    {
        public static EventusBuilder UseCosmosDB(this EventusBuilder builder, string connectionString,
            string databaseId,
            Action<EventusCosmosDBOptions>? optionsConfig = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));
            }

            var options = new EventusCosmosDBOptions(databaseId, 400, 400);

            optionsConfig?.Invoke(options);

            builder.Services.AddSingleton(options);

            builder.Services.AddTransient<CosmosDBStorageProvider>();
            builder.Services.AddTransient<IEventStorageProvider>(x =>
            {
                var logger = x.GetService<ILogger<EventStorageProviderLoggingDecorator>>() ??
                             throw new InvalidOperationException();
                var toDecorate = x.GetService<CosmosDBStorageProvider>() ?? throw new InvalidOperationException();
                var decorated = new EventStorageProviderLoggingDecorator(
                    toDecorate,
                    logger);
                return decorated;
            });

            builder.Services.AddTransient<CosmosDBSnapShotProvider>();
            builder.Services.AddTransient<ISnapshotStorageProvider>(x =>
            {
                var logger = x.GetService<ILogger<SnapshotProviderLoggingDecorator>>() ??
                             throw new InvalidOperationException();
                var toDecorate = x.GetService<CosmosDBSnapShotProvider>() ??
                                 throw new InvalidOperationException();
                var decorated = new SnapshotProviderLoggingDecorator(
                    toDecorate,
                    logger);
                return decorated;
            });

            var cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions {ApplicationName = "Eventus"});

            builder.Services.AddSingleton(cosmosClient);

            return builder;
        }
    }
}