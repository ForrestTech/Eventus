using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventus.Events;

namespace Eventus.Storage
{
    public static class EventStorageProviderExtensions
    {
        public static Task<IEnumerable<IEvent>> GetEventsAsync(this IEventStorageProvider provider, Type aggregateType, Guid aggregateId)
        {
            return provider.GetEventsAsync(aggregateType, aggregateId, 0, int.MaxValue);
        }

        public static Task<IEnumerable<IEvent>> GetEventsAsync(this IEventStorageProvider provider, Type aggregateType, Guid aggregateId, int start)
        {
            return provider.GetEventsAsync(aggregateType, aggregateId, start, int.MaxValue);
        }
    }
}