namespace Eventus.Events
{
    using MassTransit;
    using System;

    /// <summary>
    /// A common base event for all events that applied to Eventus aggregates 
    /// </summary>
    public class Event : IEvent
    {
        public Guid EventId { get; set; }

        public Guid AggregateId { get; set; }
        
        public int TargetVersion { get; set; }

        public DateTime EventCommittedTimestamp { get; set; }

        public int EventVersion { get; set; }

        public Event()
        {
        }

        /// <summary>
        /// This should be the most common event constructor as we always need an aggregate ID and target version but we can generate most other parameters
        /// </summary>
        protected Event(Guid aggregateId, int targetVersion) : this(aggregateId, targetVersion, NewId.NextGuid())
        {
        }

        protected Event(Guid aggregateId, int targetVersion, Guid eventId) : this(aggregateId, targetVersion, eventId, 1)
        {
        }


        private Event(Guid aggregateId, int targetVersion, Guid eventId, int eventEventVersion)
        {
            AggregateId = aggregateId;
            TargetVersion = targetVersion;
            EventVersion = eventEventVersion;
            EventId = eventId;
        }
    }
}