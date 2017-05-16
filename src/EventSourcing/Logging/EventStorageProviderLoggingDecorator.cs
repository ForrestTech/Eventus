using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Domain;
using EventSourcing.Events;
using EventSourcing.Storage;

namespace EventSourcing.Logging
{
    public class EventStorageProviderLoggingDecorator : LoggingDecorator, IEventStorageProvider
    {
        private readonly IEventStorageProvider _decorated;

        protected override string TypeName => "Event Storage Provider";

        public EventStorageProviderLoggingDecorator(IEventStorageProvider decorated)
        {
            _decorated = decorated;
        }

        public Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start = 0, int count = int.MaxValue)
        {
            return LogMethodCallAsync(() => _decorated.GetEventsAsync(aggregateType, aggregateId, start, count), new object[] { aggregateType, aggregateId, start, count });
        }

        public Task<IEvent> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            return LogMethodCallAsync(() => _decorated.GetLastEventAsync(aggregateType, aggregateId), new object[] { aggregateType, aggregateId });
        }

        public Task CommitChangesAsync(Aggregate aggregate)
        {
            return LogMethodCallAsync(() => _decorated.CommitChangesAsync(aggregate), aggregate);
        }
    }
}