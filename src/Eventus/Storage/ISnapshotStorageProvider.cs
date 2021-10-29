using System;
using System.Threading.Tasks;

namespace Eventus.Storage
{
    public interface ISnapshotStorageProvider
    {
        Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version);

        Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId);

        Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot);
    }
}