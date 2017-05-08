using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using EventSourcing.Domain;
using EventSourcing.Event;
using EventSourcing.Repository;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace EventSourcing.DocumentDb
{
    public class DocumentDbStorageProvider : IEventStorageProvider
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;
        private static JsonSerializerSettings _serializerSetting;

        public DocumentDbStorageProvider(DocumentClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        public async Task InitAsync(DocumentDbEventStoreConfig config)
        {
            await CreateDatabaseIfNotExistsAsync();

            foreach (var c in config.AggregateConfig)
            {
                await CreateCollectionIfNotExistsAsync(c.AggregateType.Name, c.OfferThroughput);
            }
        }

        public async Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start, int count)
        {
            try
            {
                var collectionUri = CollectionUri(aggregateType);

                var query = _client.CreateDocumentQuery<DocumentDbEventStoreEvent>(
                        collectionUri,
                        new FeedOptions { MaxItemCount = -1 })
                    .Where(x => x.AggregateId == aggregateId)
                    .OrderBy(x => x.EventTimestamp)
                    .AsDocumentQuery();

                var results = new List<IEvent>();
                while (query.HasMoreResults)
                {
                    var items = await query.ExecuteNextAsync<DocumentDbEventStoreEvent>();

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
                var collectionUri = CollectionUri(aggregateType);

                var query = _client.CreateDocumentQuery<DocumentDbEventStoreEvent>(
                        collectionUri,
                        new FeedOptions { MaxItemCount = -1 })
                        .Where(x => x.AggregateId == aggregateId)
                        .OrderByDescending(x => x.EventTimestamp)
                        .Take(1)
                    .AsDocumentQuery();

                var result = await query.ExecuteNextAsync<DocumentDbEventStoreEvent>();
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
                var collectionUri = CollectionUri(aggregate.GetType());

                var commited = aggregate.LastCommittedVersion;

                foreach (var @event in events)
                {
                    commited++;
                    var documetEvent = CreateDocumentDbEvent(aggregate, @event, commited);
                    await _client.CreateDocumentAsync(collectionUri, documetEvent);
                }
            }
        }

        private Uri CollectionUri(Type aggregateType)
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, aggregateType.Name);
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _client.CreateDatabaseAsync(new Database { Id = _databaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateCollectionIfNotExistsAsync(string collectionId, int throughput)
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //todo lookup throughput from config
                    var collection = new DocumentCollection
                    {
                        Id = collectionId,
                        IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 })
                        {
                            IndexingMode = IndexingMode.Lazy
                        }
                    };

                    collection.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath
                    {
                        Path = "/eventData/*"
                    });

                    collection.PartitionKey.Paths.Add("/aggregateId");

                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_databaseId),
                        collection,
                        new RequestOptions { OfferThroughput = throughput });
                }
                else
                {
                    throw;
                }
            }
        }

        private static DocumentDbEventStoreEvent CreateDocumentDbEvent(Aggregate aggregate, IEvent @event, int commitNumber)
        {
            var docStoreEvent = new DocumentDbEventStoreEvent
            {
                Id = @event.CorrelationId,
                AggregateId = aggregate.Id,
                ClrType = GetClrTypeName(@event),
                Commited = commitNumber,
                EventTimestamp = @event.EventCommittedTimestamp,
                EventData = SerializeEvent(@event)
            };

            return docStoreEvent;
        }
      
        protected static string SerializeEvent(IEvent @event)
        {
            return JsonConvert.SerializeObject(@event, GetSerializerSettings());
        }

        protected static IEvent DeserializeEvent(DocumentDbEventStoreEvent returnedEvent)
        {
            var returnType = Type.GetType(returnedEvent.ClrType);

            return (Event.Event)JsonConvert.DeserializeObject(returnedEvent.EventData, returnType, GetSerializerSettings());
        }

        private static JsonSerializerSettings GetSerializerSettings()
        {
            return _serializerSetting ?? (_serializerSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
        }

        private static string GetClrTypeName(object @event)
        {
            return @event.GetType() + "," + @event.GetType().Assembly.GetName().Name;
        }
    }
}