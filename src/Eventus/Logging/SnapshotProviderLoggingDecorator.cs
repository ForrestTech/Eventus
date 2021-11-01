using Eventus.Configuration;

namespace Eventus.Logging
{
    using System;
    using System.Threading.Tasks;
    using Storage;
    using Microsoft.Extensions.Logging;

    public class SnapshotProviderLoggingDecorator : LoggingDecorator, ISnapshotStorageProvider
    {
        private readonly ISnapshotStorageProvider _decorated;

        private const string TypeName = "Snapshot Storage Provider";

        public SnapshotProviderLoggingDecorator(ISnapshotStorageProvider decorated,
            ILogger<SnapshotProviderLoggingDecorator> logger,
            EventusOptions options) : base(logger, options)
        {
            _decorated = decorated;
        }

        public Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            return LogMethodCallAsync(TypeName, () => _decorated.GetSnapshotAsync(aggregateType, aggregateId, version),
                new object[] {aggregateType, aggregateId, version});
        }

        public Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            return LogMethodCallAsync(TypeName, () => _decorated.GetSnapshotAsync(aggregateType, aggregateId),
                new object[] {aggregateType, aggregateId});
        }

        public Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            return LogMethodCallAsync(TypeName, () => _decorated.SaveSnapshotAsync(aggregateType, snapshot),
                new object[] {aggregateType, snapshot});
        }
    }
}
