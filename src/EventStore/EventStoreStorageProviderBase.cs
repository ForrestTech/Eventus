namespace Eventus.EventStore
{
    using Configuration;
    using System;
    using System.Text;
    using Events;
    using Storage;
    using global::EventStore.ClientAPI;
    using System.Text.Json;

    public abstract class EventStoreStorageProviderBase
    {
        private readonly EventusEventStoreOptions _eventStoreOptions;
        private readonly EventusOptions _options;
        
        protected readonly IEventStoreConnection Connection;

        protected EventStoreStorageProviderBase(IEventStoreConnection connection, 
            EventusEventStoreOptions eventStoreOptions,
            EventusOptions options)
        {
            Connection = connection;
            _eventStoreOptions = eventStoreOptions;
            _options = options;
        }

        protected string AggregateIdToStreamName(Type t, Guid id)
        {
            //Ensure first character of type name is in lower camel case
            var prefix = _eventStoreOptions.StreamPrefix;

            return $"{char.ToLower(prefix[0])}{prefix.Substring(1)}-{t.Name.ToLower()}-{id:N}";
        }

        protected IEvent DeserializeEvent(ResolvedEvent returnedEvent)
        {
            var header = JsonSerializer.Deserialize<EventStoreMetaDataHeader>(
                Encoding.UTF8.GetString(returnedEvent.Event.Metadata), _options.JsonSerializerOptions);

            var returnType = Type.GetType(header!.ClrType);

            var deserialize = JsonSerializer.Deserialize(Encoding.UTF8.GetString(returnedEvent.Event.Data), returnType!,
                _options.JsonSerializerOptions);

            return (Event)deserialize!;
        }

        protected EventData SerializeEvent(IEvent @event, int commitNumber)
        {
            var header = new EventStoreMetaDataHeader {ClrType = GetClrTypeName(@event), CommitNumber = commitNumber};

            return new EventData(@event.EventId, @event.GetType().Name, true,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, @event.GetType(), _options.JsonSerializerOptions)),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header, _options.JsonSerializerOptions)));
        }

        protected Snapshot? DeserializeSnapshotEvent(ResolvedEvent returnedEvent)
        {
            var header = JsonSerializer.Deserialize<EventStoreMetaDataHeader>(
                Encoding.UTF8.GetString(returnedEvent.Event.Metadata), _options.JsonSerializerOptions);

            var returnType = Type.GetType(header!.ClrType);

            var deserialize = JsonSerializer.Deserialize(Encoding.UTF8.GetString(returnedEvent.Event.Data),
                returnType ?? throw new InvalidOperationException(),
                _options.JsonSerializerOptions);

            return (Snapshot)deserialize!;
        }

        protected EventData SerializeSnapshotEvent(Snapshot @event, int commitNumber)
        {
            var header = new EventStoreMetaDataHeader {ClrType = GetClrTypeName(@event), CommitNumber = commitNumber};

            return new EventData(@event.Id, @event.GetType().Name, true,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, @event.GetType(), _options.JsonSerializerOptions)),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header, _options.JsonSerializerOptions)));
        }

        private static string GetClrTypeName(object @event)
        {
            return @event.GetType() + "," + @event.GetType().Assembly.GetName().Name;
        }
    }
}