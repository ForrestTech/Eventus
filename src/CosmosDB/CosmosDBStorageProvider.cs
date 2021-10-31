namespace Eventus.SqlServer
{
    using Configuration;
    using Domain;
    using Events;
    using Eventus.Configuration;
    using Microsoft.Azure.Cosmos;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class CosmosDBStorageProvider : CosmosDBProviderBase, IEventStorageProvider
    {
        public CosmosDBStorageProvider(CosmosClient client,
            EventusCosmosDBOptions cosmosOptions,
            EventusOptions options) : base(client, cosmosOptions, options)
        {
        }

        public async Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start,
            int count)
        {
            try
            {
                var container = await GetContainer(aggregateType, aggregateId);

                var sqlQueryText =
                    $"SELECT Top {count} * FROM c WHERE c.AggregateId = '{aggregateId}' ORDER BY c.Version DESC";
                var queryDefinition = new QueryDefinition(sqlQueryText);

                var queryResultSetIterator = container.GetItemQueryIterator<CosmosDBAggregateEvent>(queryDefinition);

                var events = new List<CosmosDBAggregateEvent>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    events.AddRange(currentResultSet);
                }

                return events.Select(DeserializeEvent);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return new List<IEvent>();
                }

                throw;
            }
        }

        public async Task<IEvent?> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            try
            {
                var container = await GetContainer(aggregateType, aggregateId);

                var sqlQueryText =
                    $"SELECT Top 1 * FROM c WHERE c.AggregateId = '{aggregateId}' ORDER BY c.Version DESC";
                var queryDefinition = new QueryDefinition(sqlQueryText);

                var queryResultSetIterator = container.GetItemQueryIterator<CosmosDBAggregateEvent>(queryDefinition);

                var events = new List<CosmosDBAggregateEvent>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    events.AddRange(currentResultSet);
                }

                var lastEvent = events.SingleOrDefault();

                return lastEvent == null ? null : DeserializeEvent(lastEvent);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
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
                var committed = aggregate.LastCommittedVersion;

                var container = await GetContainer(aggregate.GetType(), aggregate.Id);

                foreach (var @event in events)
                {
                    committed++;
                    var aggregateEvent = CreateCosmosDBEvent(aggregate, @event, committed);

                    await container.CreateItemAsync(aggregateEvent, new PartitionKey(aggregate.Id.ToString()));
                }
            }
        }

        private CosmosDBAggregateEvent CreateCosmosDBEvent(Aggregate aggregate, IEvent @event, int commitNumber)
        {
            var docStoreEvent = new CosmosDBAggregateEvent
            {
                Id = @event.EventId,
                AggregateId = aggregate.Id,
                ClrType = GetClrTypeName(@event),
                Version = commitNumber,
                TargetVersion = @event.TargetVersion,
                Timestamp = @event.EventCommittedTimestamp,
                Data = SerializeEvent(@event)
            };

            return docStoreEvent;
        }

        private string SerializeEvent(IEvent @event)
        {
            return JsonSerializer.Serialize(@event, @event.GetType(), Options.JsonSerializerOptions);
        }

        private IEvent DeserializeEvent(CosmosDBAggregateEvent returnedAggregateEvent)
        {
            var returnType = Type.GetType(returnedAggregateEvent.ClrType);

            var deserialize = JsonSerializer.Deserialize(returnedAggregateEvent.Data,
                returnType ?? throw new InvalidOperationException(), Options.JsonSerializerOptions);

            return (IEvent)deserialize!;
        }
    }
}