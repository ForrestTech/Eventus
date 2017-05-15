using System;

namespace EventSourcing.Exceptions
{
    public class AggregateCreationException : Exception
    {
        public AggregateCreationException(Guid aggregateId, int version)
            : base($"Aggregate {aggregateId} can't be created as it already exists with version {version + 1}")
        { }
    }
}