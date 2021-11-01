namespace Eventus.SqlServer
{
    using Extensions.DependencyInjection;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Storage;
    using System;

    public static class EventusBuilderExtensions
    {
        public static EventusBuilder UseSqlServer(this EventusBuilder builder, string connectionString,
            Action<EventusSqlServerOptions>? optionsConfig = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));
            }

            var options = new EventusSqlServerOptions(connectionString);

            optionsConfig?.Invoke(options);

            builder.Services.AddSingleton(options);

            builder.Services.AddTransient<SqlServerEventStorageProvider>();
            builder.Services.AddTransient<IEventStorageProvider>(x =>
            {
                var logger = x.GetService<ILogger<EventStorageProviderLoggingDecorator>>() ??
                             throw new InvalidOperationException();
                var toDecorate = x.GetService<SqlServerEventStorageProvider>() ?? throw new InvalidOperationException();
                var decorated = new EventStorageProviderLoggingDecorator(
                    toDecorate,
                    logger,
                    builder.EventusOptions);
                return decorated;
            });

            builder.Services.AddTransient<SqlServerSnapshotStorageProvider>();
            builder.Services.AddTransient<ISnapshotStorageProvider>(x =>
            {
                var logger = x.GetService<ILogger<SnapshotProviderLoggingDecorator>>() ??
                             throw new InvalidOperationException();
                var toDecorate = x.GetService<SqlServerSnapshotStorageProvider>() ??
                                 throw new InvalidOperationException();
                var decorated = new SnapshotProviderLoggingDecorator(
                    toDecorate,
                    logger,
                    builder.EventusOptions);
                return decorated;
            });

            builder.Services.AddTransient<IDatabaseConnectionLogger, DatabaseConnectionLogger>();

            var initialiser = new SqlProviderInitialiser(options);
            initialiser.Init();

            return builder;
        }
    }
}
