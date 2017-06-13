using System;

namespace Eventus.Events
{
    public class Event : IEvent
    {
        public int TargetVersion { get; set; }

        public Guid AggregateId { get; set; }

        public Guid CorrelationId { get; set; }

        public DateTime EventCommittedTimestamp { get; set; }

        public int ClassVersion { get; set; }

        public Event()
        {
        }

        public Event(Guid aggregateId, int targetVersion) : this(aggregateId, targetVersion, Guid.NewGuid())
        {
        }

        public Event(Guid aggregateId, int targetVersion, Guid correlationId) : this(aggregateId, targetVersion, correlationId, 1)
        {
        }


        public Event(Guid aggregateId, int targetVersion, Guid correlationId, int eventClassVersion)
        {
            AggregateId = aggregateId;
            TargetVersion = targetVersion;
            ClassVersion = eventClassVersion;
            CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId;
        }
    }
}