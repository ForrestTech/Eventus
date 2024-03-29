using System;

namespace Eventus.Events
{
    /// <summary>
    /// All events that are applied to Eventus aggregates 
    /// </summary>
    public interface IEvent 
    {
        /// <summary>
        /// Unique Id to correlate actions
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// The aggregateID of the aggregate
        /// </summary>
        Guid AggregateId { get; }

        /// <summary>
        /// Target version of the Aggregate this event will be applied against.  This should almost always be one behind the current event version number.  This starts at -1 for fresh aggregates
        /// </summary>
        int TargetVersion { get; }

        /// <summary>
        /// This is used to timestamp the event when it get's committed
        /// </summary>
        DateTime EventCommittedTimestamp { get; }

        /// <summary>
        /// This is used to handle versioning of events over time when refactoring or feature additions are done
        /// </summary>
        int EventVersion { get; }
    }
}
