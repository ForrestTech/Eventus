namespace Eventus.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Events;

    /// <summary>
    /// Storage for events that are applied to Eventus Aggregates
    /// </summary>
    public interface IEventStorageProvider
    {
        /// <summary>
        /// Get a list of events for a given aggregate
        /// </summary>
        /// <param name="aggregateType">The type of aggregate we are querying</param>
        /// <param name="aggregateId">The aggregate ID</param>
        /// <param name="offSet">The offset to start getting events from. 1 is the start</param>
        /// <param name="count">The number of events to return</param>
        /// <returns>A list of events for the aggregate</returns>
        Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int offSet, int count);

        /// <summary>
        /// Get the latest event for a given aggregate
        /// </summary>
        /// <param name="aggregateType">The type of aggregate we are querying</param>
        /// <param name="aggregateId">The aggregate ID</param>
        /// <returns>The latest event for the aggregate</returns>
        Task<IEvent?> GetLastEventAsync(Type aggregateType, Guid aggregateId);

        /// <summary>
        /// Commit changes for the provided aggregate to the storage provider.
        /// </summary>
        /// <param name="aggregate">The aggregate to save</param>
        Task CommitChangesAsync(Aggregate aggregate);
    }
}