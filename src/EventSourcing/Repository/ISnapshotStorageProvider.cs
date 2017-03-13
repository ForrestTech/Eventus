using System;
using System.Threading.Tasks;

namespace EventSourcing.Repository
{
    public interface ISnapshotStorageProvider
    {
        int SnapshotFrequency { get; }

        Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version);

        Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId);

        Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot);
    }
}