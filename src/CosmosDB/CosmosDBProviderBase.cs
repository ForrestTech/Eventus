namespace Eventus.SqlServer
{
    using Configuration;
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public abstract class CosmosDBProviderBase
    {
        private readonly CosmosClient _client;
        private readonly EventusCosmosDBOptions _options;
        private static Database? _database;
        private Container? _container;
        private Container? _snapshotContainer;

        protected CosmosDBProviderBase(CosmosClient client, EventusCosmosDBOptions options)
        {
            _client = client;
            _options = options;
        }

        protected static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        private async Task<Database> GetDatabase()
        {
            if (_database != null)
            {
                return _database;
            }
            //TODO pass in throughput settings
            _database = await _client.CreateDatabaseIfNotExistsAsync(_options.DatabaseId);
            return _database;
        }

        protected async Task<Container> GetContainer(Type aggregateType, Guid aggregateId)
        {
            if (_container != null)
            {
                return _container;
            }

            //TODO pass in throughput settings
            var database = await GetDatabase(); 
            _container = await database.CreateContainerIfNotExistsAsync(GetContainerId(aggregateType, aggregateId), "/AggregateId",400);
            return _container;
        }
        
        protected async Task<Container> GetSnapshotContainer(Type aggregateType, Guid aggregateId)
        {
            if (_snapshotContainer != null)
            {
                return _snapshotContainer;
            }

            //TODO pass in throughput settings\
            var database = await GetDatabase();
            _snapshotContainer = await database.CreateContainerIfNotExistsAsync(SnapshotContainerId(aggregateType, aggregateId), "/AggregateId",400);
            return _snapshotContainer;

        }

        protected static string GetContainerId(MemberInfo aggregateType, Guid aggregateId)
        {
            return $"{aggregateType.Name.ToLower()}-{aggregateId}";
        }

        protected static string SnapshotContainerId(Type aggregateType, Guid aggregateId)
        {
            return $"{GetContainerId(aggregateType, aggregateId)}-snapshot";
        }

        protected static string GetClrTypeName(object item)
        {
            return item.GetType() + "," + item.GetType().Assembly.GetName().Name;
        }
    }
}