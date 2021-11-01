namespace Eventus.CosmosDB
{
    using Configuration;
    using Eventus.Configuration;
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    public abstract class CosmosDBProviderBase
    {
        private readonly CosmosClient _client;
        private static Database? _database;
        private Container? _container;
        private Container? _snapshotContainer;
        private readonly EventusCosmosDBOptions _cosmosOptions;

        protected readonly EventusOptions Options;

        protected CosmosDBProviderBase(CosmosClient client,
            EventusCosmosDBOptions cosmosOptions,
            EventusOptions options)
        {
            _client = client;
            Options = options;
            _cosmosOptions = cosmosOptions;
        }

        private async Task<Database> GetDatabase()
        {
            if (_database != null)
            {
                return _database;
            }

            if (_cosmosOptions.DatabaseSharedThroughPut != null)
            {
                _database = await _client.CreateDatabaseIfNotExistsAsync(_cosmosOptions.DatabaseId,
                    _cosmosOptions.DatabaseSharedThroughPut);
            }
            else
            {
                _database = await _client.CreateDatabaseIfNotExistsAsync(_cosmosOptions.DatabaseId);
            }

            return _database;
        }

        protected async Task<Container> GetContainer(Type aggregateType, Guid aggregateId)
        {
            if (_container != null)
            {
                return _container;
            }

            var database = await GetDatabase();

            var containerProperties =
                new ContainerProperties(GetContainerId(aggregateType, aggregateId), _cosmosOptions.PartitionKey);
            
            _cosmosOptions.ExcludePaths.ForEach(x =>
            {
                containerProperties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath {Path = x});
            });

            ThroughputProperties? throughputProperties = null;

            var aggregateThroughput = _cosmosOptions.GetAggregateThroughput(aggregateType);
            if (aggregateThroughput != null)
            {
                throughputProperties = ThroughputProperties.CreateManualThroughput(aggregateThroughput.Value);
            }

            var autoScaleThroughput = _cosmosOptions.GetAggregateAutoScaleThroughput(aggregateType);
            if (autoScaleThroughput != null)
            {
                throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(autoScaleThroughput.Value);
            }

            if (throughputProperties != null)
            {
                _container = await database.CreateContainerIfNotExistsAsync(containerProperties, throughputProperties);
            }

            _container = await database.CreateContainerIfNotExistsAsync(containerProperties);

            return _container;
        }

        protected async Task<Container> GetSnapshotContainer(Type aggregateType, Guid aggregateId)
        {
            if (_snapshotContainer != null)
            {
                return _snapshotContainer;
            }

            var database = await GetDatabase();

            var containerProperties =
                new ContainerProperties(GetSnapshotContainerId(aggregateType, aggregateId), _cosmosOptions.PartitionKey);

            _cosmosOptions.ExcludePaths.ForEach(x =>
            {
                containerProperties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath {Path = x});
            });


            ThroughputProperties? throughputProperties = null;

            var aggregateThroughput = _cosmosOptions.GetSnapshotThroughput(aggregateType);
            if (aggregateThroughput != null)
            {
                throughputProperties = ThroughputProperties.CreateManualThroughput(aggregateThroughput.Value);
            }

            var autoScaleThroughput = _cosmosOptions.GetSnapshotAutoScaleThroughput(aggregateType);
            if (autoScaleThroughput != null)
            {
                throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(autoScaleThroughput.Value);
            }

            if (throughputProperties != null)
            {
                _snapshotContainer = await database.CreateContainerIfNotExistsAsync(containerProperties, throughputProperties);
            }

            _snapshotContainer = await database.CreateContainerIfNotExistsAsync(containerProperties);

            return _snapshotContainer;
        }

        private static string GetContainerId(MemberInfo aggregateType, Guid aggregateId)
        {
            return $"{aggregateType.Name.ToLower()}";
        }

        private static string GetSnapshotContainerId(MemberInfo aggregateType, Guid aggregateId)
        {
            return $"{GetContainerId(aggregateType, aggregateId)}-snapshot";
        }

        protected static string GetClrTypeName(object item)
        {
            return item.GetType() + "," + item.GetType().Assembly.GetName().Name;
        }
    }
}