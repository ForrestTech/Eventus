using System;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Repository;
using EventStore.ClientAPI;

namespace EventSourcing.EventSource
{
    public class EventstoreSnapshotStorageProvider : EventstoreStorageProviderBase, ISnapshotStorageProvider
    {
        public EventstoreSnapshotStorageProvider(IEventStoreConnection connection, Func<string> getStreamNamePrefix, int snapshotFrequency)
            : base(connection, getStreamNamePrefix)
        {
            SnapshotFrequency = snapshotFrequency;
        }

        public int SnapshotFrequency { get; }

        public async Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            Snapshot snapshot = null;

            var streamEvents = await Connection.ReadStreamEventsBackwardAsync(SnapShotStreamName(aggregateType, aggregateId), StreamPosition.End, 1, false);

            if (streamEvents.Events.Any())
            {
                var result = streamEvents.Events.FirstOrDefault();

                snapshot = DeserializeSnapshotEvent(result);
            }

            return snapshot;
        }

        public async Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            var snapshotyEvent = SerializeSnapshotEvent(snapshot, snapshot.Version);

            await Connection.AppendToStreamAsync(SnapShotStreamName(aggregateType, snapshot.AggregateId), ExpectedVersion.Any, snapshotyEvent);
        }

        public async Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            Snapshot snapshot = null;

            var streamEvents = await Connection.ReadStreamEventsBackwardAsync(SnapShotStreamName(aggregateType, aggregateId), version, 1, false);

            if (streamEvents.Events.Any())
            {
                var result = streamEvents.Events.FirstOrDefault();

                snapshot = DeserializeSnapshotEvent(result);
            }

            return snapshot;
        }

        private string SnapShotStreamName(Type aggregateType, Guid aggregateId)
        {
            return $"{AggregateIdToStreamName(aggregateType, aggregateId)}-snapshot";
        }
    }
}