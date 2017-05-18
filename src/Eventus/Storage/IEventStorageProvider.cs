using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventus.Domain;
using Eventus.Events;

namespace Eventus.Storage
{
    public interface IEventStorageProvider 
    {
        Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start = 0, int count = int.MaxValue);

        Task<IEvent> GetLastEventAsync(Type aggregateType, Guid aggregateId);

        Task CommitChangesAsync(Aggregate aggregate);
    }
}