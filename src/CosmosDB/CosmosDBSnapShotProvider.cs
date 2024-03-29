using Microsoft.Extensions.Logging;

namespace Eventus.CosmosDB
{
    using Configuration;
    using Eventus.Configuration;
    using Microsoft.Azure.Cosmos;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class CosmosDBSnapShotProvider : CosmosDBProviderBase, ISnapshotStorageProvider
    {
        private readonly ILogger<CosmosDBSnapShotProvider> _logger;

        public CosmosDBSnapShotProvider(CosmosClient client,
            EventusCosmosDBOptions cosmosOptions,
            EventusOptions options,
            ILogger<CosmosDBSnapShotProvider> logger) : base(client, cosmosOptions, options)
        {
            _logger = logger;
        }

        public async Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            try
            {
                var container = await GetSnapshotContainer(aggregateType, aggregateId);

                var sqlQueryText = "SELECT * FROM c WHERE c.AggregateId = @aggregateId and c.Version == @version";
                var queryDefinition = new QueryDefinition(sqlQueryText)
                    .WithParameter("@aggregateId", aggregateId)
                    .WithParameter("@version", version);

                _logger.LogDebug("Executing Cosmos Query:'{Sql}' for aggregate: '{Aggregate}'", sqlQueryText, aggregateId);

                var queryResultSetIterator = container.GetItemQueryIterator<CosmosDBSnapshot>(queryDefinition);

                var snapshots = new List<CosmosDBSnapshot>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    snapshots.AddRange(currentResultSet);
                }

                var snapshot = snapshots.SingleOrDefault();

                return snapshot == null ? null : DeserializeSnapshot(snapshot);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

        public async Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            try
            {
                var container = await GetSnapshotContainer(aggregateType, aggregateId);

                var sqlQueryText =
                    "SELECT Top 1 * FROM c WHERE c.AggregateId = @aggregateId ORDER BY c.Version DESC";
                var queryDefinition = new QueryDefinition(sqlQueryText)
                    .WithParameter("@aggregateId", aggregateId);

                _logger.LogDebug("Executing Cosmos Query:'{Sql}' for aggregate: '{Aggregate}'", sqlQueryText, aggregateId);

                var queryResultSetIterator = container.GetItemQueryIterator<CosmosDBSnapshot>(queryDefinition);

                var snapshots = new List<CosmosDBSnapshot>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    snapshots.AddRange(currentResultSet);
                }

                var snapshot = snapshots.SingleOrDefault();

                return snapshot == null ? null : DeserializeSnapshot(snapshot);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

        public async Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            var container = await GetSnapshotContainer(aggregateType, snapshot.AggregateId);
            var cosmosDBSnapshot = CreateSnapshotEvent(snapshot);

            _logger.LogDebug("Inserting snapshot for aggregate: '{Aggregate}'", snapshot.AggregateId);

            await container.CreateItemAsync(cosmosDBSnapshot, new PartitionKey(snapshot.AggregateId.ToString()));
        }

        private CosmosDBSnapshot CreateSnapshotEvent(Snapshot snapshot)
        {
            return new CosmosDBSnapshot
            {
                Id = snapshot.Id,
                AggregateId = snapshot.AggregateId,
                ClrType = GetClrTypeName(snapshot),
                Version = snapshot.Version,
                Timestamp = Clock.Now(),
                Data = SerializeSnapshot(snapshot)
            };
        }

        private string SerializeSnapshot(Snapshot snapshot)
        {
            var serialized = JsonSerializer.Serialize(snapshot, snapshot.GetType(), Options.JsonSerializerOptions);
            return serialized;
        }

        private Snapshot DeserializeSnapshot(CosmosDBEventBase item)
        {
            var returnType = Type.GetType(item.ClrType);

            var deserialized = JsonSerializer.Deserialize(item.Data,
                returnType ?? throw new InvalidOperationException(), Options.JsonSerializerOptions);

            return (Snapshot)deserialized!;
        }
    }
}
