using System;

namespace EventSourcing.Exceptions
{
    public class AggregateCreationException : Exception
    {
        public AggregateCreationException(Guid correlationId, int version) 
            : base($"Aggregate { correlationId} can't be created as it already exists with version {version + 1}")
        {}
    }
}