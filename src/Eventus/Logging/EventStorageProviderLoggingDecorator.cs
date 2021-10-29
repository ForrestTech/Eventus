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

        protected override string TypeName => "Event Storage Provider";

        public EventStorageProviderLoggingDecorator(IEventStorageProvider decorated, ILogger logger) : base(logger)
        {
            _decorated = decorated;
        }

        public Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start, int count)
        {
            return LogMethodCallAsync(() => _decorated.GetEventsAsync(aggregateType, aggregateId, start, count),
                new object[] {aggregateType, aggregateId, start, count});
        }

        public Task<IEvent?> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            return LogMethodCallAsync(() => _decorated.GetLastEventAsync(aggregateType, aggregateId),
                new object[] {aggregateType, aggregateId});
        }

        public Task CommitChangesAsync(Aggregate aggregate)
        {
            return LogMethodCallAsync(() => _decorated.CommitChangesAsync(aggregate), aggregate);
        }
    }
}