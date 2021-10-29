namespace Eventus.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Events;

    public interface IEventStorageProvider
    {
        Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start, int count);

        Task<IEvent?> GetLastEventAsync(Type aggregateType, Guid aggregateId);

        Task CommitChangesAsync(Aggregate aggregate);
    }
}