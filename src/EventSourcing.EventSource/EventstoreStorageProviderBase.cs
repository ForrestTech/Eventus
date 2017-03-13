using System;
using System.Text;
using EventSourcing.Event;
using EventSourcing.Repository;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace EventSourcing.EventSource
{
    public abstract class EventstoreStorageProviderBase
    {
        private readonly Func<string> _getStreamNamePrefix;
        private static JsonSerializerSettings _serializerSetting;

        protected readonly IEventStoreConnection Connection;

        protected EventstoreStorageProviderBase(IEventStoreConnection connection, Func<string> getStreamNamePrefix)
        {
            Connection = connection;
            _getStreamNamePrefix = getStreamNamePrefix;
        }

        protected string AggregateIdToStreamName(Type t, Guid id)
        {
            //Ensure first character of type name is in lower camel case
            var prefix = _getStreamNamePrefix();

            return $"{char.ToLower(prefix[0])}{prefix.Substring(1)}-{t.Name}-{id:N}";
        }

        private static JsonSerializerSettings GetSerializerSettings()
        {
            return _serializerSetting ?? (_serializerSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
        }

        protected static IEvent DeserializeEvent(ResolvedEvent returnedEvent)
        {

            var header = JsonConvert.DeserializeObject<EventstoreMetaDataHeader>(
                Encoding.UTF8.GetString(returnedEvent.Event.Metadata), GetSerializerSettings());

            var returnType = Type.GetType(header.ClrType);

            return
                (Event.Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(returnedEvent.Event.Data), returnType,
                    GetSerializerSettings());
        }

        protected static EventData SerializeEvent(IEvent @event, int commitNumber)
        {
            var header = new EventstoreMetaDataHeader()
            {
                ClrType = GetClrTypeName(@event),
                CommitNumber = commitNumber
            };

            return new EventData(@event.CorrelationId, @event.GetType().Name, true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, GetSerializerSettings())),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header, GetSerializerSettings())));
        }

        protected static Snapshot DeserializeSnapshotEvent(ResolvedEvent returnedEvent)
        {

            var header = JsonConvert.DeserializeObject<EventstoreMetaDataHeader>(
                Encoding.UTF8.GetString(returnedEvent.Event.Metadata), GetSerializerSettings());

            var returnType = Type.GetType(header.ClrType);

            return
                (Snapshot)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(returnedEvent.Event.Data), returnType,
                    GetSerializerSettings());
        }

        protected static EventData SerializeSnapshotEvent(Snapshot @event, int commitNumber)
        {
            var header = new EventstoreMetaDataHeader
            {
                ClrType = GetClrTypeName(@event),
                CommitNumber = commitNumber
            };

            return new EventData(@event.Id, @event.GetType().Name, true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, GetSerializerSettings())),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header, GetSerializerSettings())));
        }

        private static string GetClrTypeName(object @event)
        {
            return @event.GetType() + "," + @event.GetType().Assembly.GetName().Name;
        }
    }
}