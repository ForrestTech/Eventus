using System;
using System.Threading.Tasks;

namespace Eventus.Storage
{
    /// <summary>
    /// Storage for events that are applied to Eventus Aggregates
    /// </summary>
    public interface ISnapshotStorageProvider
    {
        /// <summary>
        /// Get a specific snapshot version for an aggregate
        /// </summary>
        /// <param name="aggregateType">The type of the aggregate we want the Snapshot for</param>
        /// <param name="aggregateId">The id of the aggregate who Snapshot we want to load</param>
        /// <param name="version">The version of the Snapshot to load</param>
        /// <returns>The specific version Snapshot</returns>
        Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version);

        /// <summary>
        /// Get the latest snapshot for the aggregate
        /// </summary>
        /// <param name="aggregateType">The type of the aggregate we want the Snapshot for</param>
        /// <param name="aggregateId">The id of the aggregate who Snapshot we want to load</param>
        /// <returns>The latest Snapshot</returns>
        Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId);

        /// <summary>
        /// Saves the provided Snapshot to the storage provider
        /// </summary>
        /// <param name="aggregateType">The type of the aggregate we want the Snapshot for</param>
        /// <param name="snapshot">The Snapshot to save</param>
        Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot);
    }
}