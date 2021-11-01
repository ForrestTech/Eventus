namespace Eventus.EventStore
{
    using Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using Events;
    using Storage;
    using global::EventStore.ClientAPI;

    public class EventStoreStorageProvider : EventStoreStorageProviderBase, IEventStorageProvider
    {
        //There is a max limit of 4096 messages per read in eventstore so use paging
        private const int EventStorePageSize = 200;

        public EventStoreStorageProvider(IEventStoreConnection connection,
            EventusEventStoreOptions eventStoreOptions,
            EventusOptions options)
            : base(connection, eventStoreOptions, options)
        {
        }

        public async Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start,
            int count)
        {
            var events = await ReadEventsAsync(aggregateType, aggregateId, start, count);

            return events;
        }

        public async Task<IEvent?> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            var streamName = AggregateIdToStreamName(aggregateType, aggregateId);
            var results = await Connection.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);

            if (results.Status == SliceReadStatus.Success && results.Events.Length > 0)
            {
                return DeserializeEvent(results.Events.First());
            }

            return null;
        }

        public async Task CommitChangesAsync(Aggregate aggregate)
        {
            var events = aggregate.GetUncommittedChanges().ToList();

            if (events.Any())
            {
                var streamName = AggregateIdToStreamName(aggregate.GetType(), aggregate.Id);
                var lastVersion = aggregate.LastCommittedVersion;
                var expectedVersion = lastVersion == 0 ? ExpectedVersion.NoStream : lastVersion -1;
                
                var eventData = events.Select(@event => SerializeEvent(@event, aggregate.LastCommittedVersion + 1))
                    .ToList();

                await Connection.AppendToStreamAsync(streamName,
                    expectedVersion,
                    eventData);
            }
        }

        private async Task<IEnumerable<IEvent>> ReadEventsAsync(Type aggregateType, Guid aggregateId, int start,
            int count)
        {
            var streamEvents = new List<ResolvedEvent>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = start < 0 ? StreamPosition.Start : start;

            //Read the stream using pagesize which was set before.
            //We only need to read the full page ahead if expected results are larger than the page size
            do
            {
                var nextReadCount = count - streamEvents.Count;

                if (nextReadCount == 0)
                    break;

                if (nextReadCount > EventStorePageSize)
                {
                    nextReadCount = EventStorePageSize;
                }

                currentSlice = await Connection.ReadStreamEventsForwardAsync(
                    $"{AggregateIdToStreamName(aggregateType, aggregateId)}", nextSliceStart, nextReadCount, false);

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            return streamEvents.Select(DeserializeEvent).ToList();
        }
    }
}
