namespace Eventus.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Events;
    using Storage;
    using Microsoft.Extensions.Logging;
    
    public class EventStorageProviderLoggingDecorator : LoggingDecorator, IEventStorageProvider
    {
        private readonly IEventStorageProvider _decorated;

        private const string TypeName = "Event Storage Provider";

        public EventStorageProviderLoggingDecorator(IEventStorageProvider decorated, ILogger<EventStorageProviderLoggingDecorator> logger) : base(logger)
        {
            _decorated = decorated;
        }

        public Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int offSet, int count)
        {
            return LogMethodCallAsync(TypeName, () => _decorated.GetEventsAsync(aggregateType, aggregateId, offSet, count),
                new object[] {aggregateType, aggregateId, offSet, count});
        }

        public Task<IEvent?> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            return LogMethodCallAsync(TypeName, () => _decorated.GetLastEventAsync(aggregateType, aggregateId),
                new object[] {aggregateType, aggregateId});
        }

        public Task CommitChangesAsync(Aggregate aggregate)
        {
            return LogMethodCallAsync(TypeName, () => _decorated.CommitChangesAsync(aggregate), aggregate);
        }
    }
}