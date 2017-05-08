using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

            //todo move to an async init method (make common interface for storage providers that should have a application start setup)
            CreateDatabaseIfNotExistsAsync().Wait();
        }

        public async Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start, int count)
        {
            await CreateCollectionIfNotExistsAsync(aggregateType, aggregateId);

            try
            {
                var query = _client.CreateDocumentQuery<DocumentDbEventStoreEvent>(
                        UriFactory.CreateDocumentCollectionUri(_databaseId, AggregateIdToCollectionName(aggregateType, aggregateId)),
                        new FeedOptions { MaxItemCount = -1 })
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
            await CreateCollectionIfNotExistsAsync(aggregateType, aggregateId);

            try
            {
                var query = _client.CreateDocumentQuery<IEvent>(
                        UriFactory.CreateDocumentCollectionUri(_databaseId, AggregateIdToCollectionName(aggregateType, aggregateId)),
                        new FeedOptions { MaxItemCount = -1 })
                        .OrderBy(x => x.EventCommittedTimestamp)
                        .Take(1)
                        .AsDocumentQuery();

                var result = await query.ExecuteNextAsync<IEvent>();
                return result.SingleOrDefault();

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
            await CreateCollectionIfNotExistsAsync(aggregate.GetType(), aggregate.Id);

            var events = aggregate.GetUncommittedChanges();

            if (events.Any())
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(_databaseId, AggregateIdToCollectionName(aggregate.GetType(), aggregate.Id));

                var commited = aggregate.LastCommittedVersion;

                foreach (var @event in events)
                {
                    commited++;
                    var documetEvent = CreateDocumentDbEvent(@event, commited);
                    await _client.CreateDocumentAsync(collectionUri, documetEvent);
                }
            }
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

        private async Task CreateCollectionIfNotExistsAsync(Type aggregateType, Guid aggregateId)
        {
            var collectionId = AggregateIdToCollectionName(aggregateType, aggregateId);

            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //todo lookup throughput from config
                    //todo remove all indexs by default and allow them to be configured
                    //todo consider a single collection partitioned on aggregate id

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
                        Path = "/EventData/*"
                    });

                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_databaseId),
                        collection,
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }

        private string AggregateIdToCollectionName(Type aggregateType, Guid aggregateId)
        {
            return $"{aggregateType.Name}-{aggregateId:N}";
        }

        private static DocumentDbEventStoreEvent CreateDocumentDbEvent(IEvent @event, int commitNumber)
        {
            var docStoreEvent = new DocumentDbEventStoreEvent
            {
                EventData = SerializeEvent(@event),
                ClrType = GetClrTypeName(@event),
                Commited = commitNumber
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

    public class DocumentDbEventStoreEvent
    {
        public string ClrType { get; set; }

        public int Commited { get; set; }

        //todo just query _ts
        public DateTime EventTimestamp { get; set; }

        public string EventData { get; set; }
    }
}