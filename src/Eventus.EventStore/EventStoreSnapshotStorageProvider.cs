using System;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Eventus.Storage;

namespace Eventus.EventStore
{
    public class EventstoreSnapshotStorageProvider : EventstoreStorageProviderBase, ISnapshotStorageProvider
    {
        public EventstoreSnapshotStorageProvider(IEventStoreConnection connection, int snapshotFrequency) : this(connection, snapshotFrequency, null)
        { }

        public EventstoreSnapshotStorageProvider(IEventStoreConnection connection, int snapshotFrequency, Func<string> getStreamNamePrefix)
            : base(connection, getStreamNamePrefix)
        {
            if (snapshotFrequency <= 2) throw new ArgumentOutOfRangeException(nameof(snapshotFrequency));

            SnapshotFrequency = snapshotFrequency;
        }

        public int SnapshotFrequency { get; }

        public async Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            Snapshot snapshot = null;

            var streamEvents = await Connection.ReadStreamEventsBackwardAsync(SnapShotStreamName(aggregateType, aggregateId), StreamPosition.End, 1, false)
                .ConfigureAwait(false);

            if (streamEvents.Events.Any())
            {
                var result = streamEvents.Events.FirstOrDefault();

                snapshot = DeserializeSnapshotEvent(result);
            }

            return snapshot;
        }

        public Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            var snapShotEvent = SerializeSnapshotEvent(snapshot, snapshot.Version);

            return Connection.AppendToStreamAsync(SnapShotStreamName(aggregateType, snapshot.AggregateId), ExpectedVersion.Any, snapShotEvent);
        }

        public async Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            Snapshot snapshot = null;

            var streamEvents = await Connection.ReadStreamEventsBackwardAsync(SnapShotStreamName(aggregateType, aggregateId), version, 1, false)
                .ConfigureAwait(false);

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