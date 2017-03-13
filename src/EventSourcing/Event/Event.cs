using System;

namespace EventSourcing.Event
{
    public class Event : IEvent
    {
        public int TargetVersion { get; set; }

        public Guid AggregateId { get; set; }

        public Guid CorrelationId { get; }

        public DateTime EventCommittedTimestamp { get; set; }

        public int ClassVersion { get; set; }

        public Event()
        {
        }

        public Event(Guid aggregateId, int targetVersion, int eventClassVersion = 1)
        {
            AggregateId = aggregateId;
            TargetVersion = targetVersion;
            ClassVersion = eventClassVersion;
            CorrelationId = Guid.NewGuid();
        }
    }
}