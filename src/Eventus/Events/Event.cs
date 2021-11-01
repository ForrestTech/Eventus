using System.Collections.Generic;
using System.Linq;

namespace Eventus.Events
{
    using MassTransit;
    using System;

    /// <summary>
    /// A common base event for all events that applied to Eventus aggregates 
    /// </summary>
    public class Event : IEvent
    {
        public Guid EventId { get; init; }

        public Guid AggregateId { get; init; }
        
        public int TargetVersion { get; init; }

        public DateTime EventCommittedTimestamp { get; init; }

        public int EventVersion { get; init; }

        /// <summary>
        /// This should be the most common event constructor as we always need an aggregate ID and target version but we can generate most other parameters
        /// </summary>
        protected Event(Guid aggregateId, int targetVersion) : this(aggregateId, targetVersion, NewId.NextGuid())
        {
        }

        protected Event(Guid aggregateId, int targetVersion, Guid eventId) : this(aggregateId, targetVersion, eventId, 1)
        {
        }

        private Event(Guid aggregateId, int targetVersion, Guid eventId, int eventVersion)
        {
            AggregateId = aggregateId;
            TargetVersion = targetVersion;
            EventVersion = eventVersion;
            EventId = eventId;
            EventCommittedTimestamp = Clock.Now();
        }
    }
}
