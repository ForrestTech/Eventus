using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.DocumentDb.Config;
using EventSourcing.Domain;
using EventSourcing.Event;
using EventSourcing.Repository;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace EventSourcing.DocumentDb
{
    public class DocumentDbStorageProvider : DocumentDbProviderBase, IEventStorageProvider
    {
        public DocumentDbStorageProvider(DocumentClient client, string databaseId) : base(client, databaseId)
        { }

        public async Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start, int count)
        {
            try
            {
                var collectionUri = EventCollectionUri(aggregateType);

                var query = Client.CreateDocumentQuery<DocumentDbAggregateEvent>(
                        collectionUri,
                        new FeedOptions { MaxItemCount = -1 })
                    .Where(x => x.AggregateId == aggregateId && x.Version >= start)
                    .OrderBy(x => x.Version)
                    .AsDocumentQuery();

                var results = new List<IEvent>();
                while (query.HasMoreResults)
                {
                    var items = await query.ExecuteNextAsync<DocumentDbAggregateEvent>();

                    results.AddRange(items.Select(DeserializeEvent));
                }

                return results;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<IEvent> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            try
            {
                var collectionUri = EventCollectionUri(aggregateType);

                var query = Client.CreateDocumentQuery<DocumentDbAggregateEvent>(
                        collectionUri,
                        new FeedOptions { MaxItemCount = -1 })
                        .Where(x => x.AggregateId == aggregateId)
                        .OrderByDescending(x => x.Version)
                        .Take(1)
                    .AsDocumentQuery();

                var result = await query.ExecuteNextAsync<DocumentDbAggregateEvent>();
                var item = result.SingleOrDefault();

                return item == null ? null : DeserializeEvent(item);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task CommitChangesAsync(Aggregate aggregate)
        {
            var events = aggregate.GetUncommittedChanges();

            if (events.Any())
            {
                var collectionUri = EventCollectionUri(aggregate.GetType());

                var commited = aggregate.LastCommittedVersion;

                foreach (var @event in events)
                {
                    commited++;
                    var documetEvent = CreateDocumentDbEvent(aggregate, @event, commited);
                    await Client.CreateDocumentAsync(collectionUri, documetEvent);
                }
            }
        }

        private static DocumentDbAggregateEvent CreateDocumentDbEvent(Aggregate aggregate, IEvent @event, int commitNumber)
        {
            var docStoreEvent = new DocumentDbAggregateEvent
            {
                Id = @event.CorrelationId,
                AggregateId = aggregate.Id,
                ClrType = GetClrTypeName(@event),
                Version = commitNumber,
                TargetVersion = @event.TargetVersion,
                Timestamp = @event.EventCommittedTimestamp,
                Data = SerializeEvent(@event)
            };

            return docStoreEvent;
        }

        protected static string SerializeEvent(IEvent @event)
        {
            return JsonConvert.SerializeObject(@event, SerializerSettings);
        }

        protected static IEvent DeserializeEvent(DocumentDbAggregateEvent returnedAggregateEvent)
        {
            var returnType = Type.GetType(returnedAggregateEvent.ClrType);

            return (Event.Event)JsonConvert.DeserializeObject(returnedAggregateEvent.Data, returnType, SerializerSettings);
        }

        protected Uri EventCollectionUri(Type aggregateType)
        {
            return UriFactory.CreateDocumentCollectionUri(DatabaseId, aggregateType.Name);
        }

        protected Uri SnapshotCollectionUri(Type aggregateType)
        {
            return UriFactory.CreateDocumentCollectionUri(DatabaseId, SnapshotCollectionName(aggregateType));
        }
    }
}