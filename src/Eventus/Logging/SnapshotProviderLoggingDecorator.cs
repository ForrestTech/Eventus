using System;
using System.Threading.Tasks;
using Eventus.Storage;

namespace Eventus.Logging
{
    public class SnapshotProviderLoggingDecorator : LoggingDecorator, ISnapshotStorageProvider
    {
        private readonly ISnapshotStorageProvider _decorated;

        protected override string TypeName => "Snapshot Storage Provider";

        public SnapshotProviderLoggingDecorator(ISnapshotStorageProvider decorated)
        {
            _decorated = decorated;
        }
        public int SnapshotFrequency => _decorated.SnapshotFrequency;

        public Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            return LogMethodCallAsync(() => _decorated.GetSnapshotAsync(aggregateType, aggregateId, version), new object[] { aggregateType, aggregateId, version });
        }

        public Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            return LogMethodCallAsync(() => _decorated.GetSnapshotAsync(aggregateType, aggregateId), new object[] { aggregateType, aggregateId });
        }

        public Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            return LogMethodCallAsync(() => _decorated.SaveSnapshotAsync(aggregateType, snapshot), new object[] { aggregateType, snapshot });
        }
    }
}