namespace Eventus.EventStore
{
    using System;
    using System.Text;
    using Events;
    using Storage;
    using global::EventStore.ClientAPI;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using JsonSerializer = System.Text.Json.JsonSerializer;

    public abstract class EventStoreStorageProviderBase
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = {new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)}
        };

        protected readonly IEventStoreConnection Connection;
        private readonly EventusEventStoreOptions _options;

        protected EventStoreStorageProviderBase(IEventStoreConnection connection, EventusEventStoreOptions options)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _options = options;
        }

        protected string AggregateIdToStreamName(Type t, Guid id)
        {
            //Ensure first character of type name is in lower camel case
            var prefix = _options.StreamPrefix;

            return $"{char.ToLower(prefix[0])}{prefix.Substring(1)}-{t.Name.ToLower()}-{id:N}";
        }

        protected static IEvent DeserializeEvent(ResolvedEvent returnedEvent)
        {
            var header = JsonSerializer.Deserialize<EventStoreMetaDataHeader>(
                Encoding.UTF8.GetString(returnedEvent.Event.Metadata), JsonSerializerOptions);

            var returnType = Type.GetType(header!.ClrType);

            var deserialize = JsonSerializer.Deserialize(Encoding.UTF8.GetString(returnedEvent.Event.Data), returnType!,
                JsonSerializerOptions);

            return (Event)deserialize!;
        }

        protected static EventData SerializeEvent(IEvent @event, int commitNumber)
        {
            var header = new EventStoreMetaDataHeader {ClrType = GetClrTypeName(@event), CommitNumber = commitNumber};

            return new EventData(@event.CorrelationId, @event.GetType().Name, true,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, @event.GetType(), JsonSerializerOptions)),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header, JsonSerializerOptions)));
        }

        protected static Snapshot? DeserializeSnapshotEvent(ResolvedEvent returnedEvent)
        {
            var header = JsonSerializer.Deserialize<EventStoreMetaDataHeader>(
                Encoding.UTF8.GetString(returnedEvent.Event.Metadata), JsonSerializerOptions);

            var returnType = Type.GetType(header!.ClrType);

            var deserialize = JsonSerializer.Deserialize(Encoding.UTF8.GetString(returnedEvent.Event.Data),
                returnType ?? throw new InvalidOperationException(),
                JsonSerializerOptions);

            return (Snapshot)deserialize!;
        }

        protected static EventData SerializeSnapshotEvent(Snapshot @event, int commitNumber)
        {
            var header = new EventStoreMetaDataHeader {ClrType = GetClrTypeName(@event), CommitNumber = commitNumber};

            return new EventData(@event.Id, @event.GetType().Name, true,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, @event.GetType(), JsonSerializerOptions)),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header, JsonSerializerOptions)));
        }

        private static string GetClrTypeName(object @event)
        {
            return @event.GetType() + "," + @event.GetType().Assembly.GetName().Name;
        }
    }
}