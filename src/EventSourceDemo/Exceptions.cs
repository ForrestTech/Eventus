using System;

namespace EventSourceDemo
{
    public class AggregateNotFoundException : Exception
    {
        public readonly Guid Id;
        public readonly Type Type;

        public AggregateNotFoundException(Guid id, Type type)
            : base($"Aggregate '{id}' (type {type.Name}) was not found.")
        {
            Id = id;
            Type = type;
        }
    }

    public class AggregateDeletedException : Exception
    {
        public readonly Guid Id;
        public readonly Type Type;

        public AggregateDeletedException(Guid id, Type type)
            : base($"Aggregate '{id}' (type {type.Name}) was deleted.")
        {
            Id = id;
            Type = type;
        }
    }

    public class AggregateVersionException : Exception
    {
        public readonly Guid Id;
        public readonly Type Type;
        public readonly int AggregateVersion;
        public readonly int RequestedVersion;

        public AggregateVersionException(Guid id, Type type, int aggregateVersion, int requestedVersion)
            : base(string.Format("Requested version {2} of aggregate '{0}' (type {1}) - aggregate version is {3}", id, type.Name, requestedVersion, aggregateVersion))
        {
            Id = id;
            Type = type;
            AggregateVersion = aggregateVersion;
            RequestedVersion = requestedVersion;
        }
    }

    public class EventDeserializationException : Exception
    {
        public readonly string EventTypeName;
        public readonly string Metadata;

        public EventDeserializationException(string eventTypeName, string metadata)
            : base($"Could not deserialize {eventTypeName} as an Event (metadata: {metadata})")
        {
            EventTypeName = eventTypeName;
            Metadata = metadata;
        }
    }
}