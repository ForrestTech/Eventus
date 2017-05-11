using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.DocumentDb.Config;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace EventSourcing.DocumentDb
{
    public abstract class DocumentDbProviderBase
    {
        private const string PartitionKey = "/aggregateId";
        private static JsonSerializerSettings _serializerSetting;

        private static readonly List<string> ExcludePaths = new List<string>
        {
            "/data/*",
            "/clrType/*",
            "/targetVersion/*",
            "/timestamp/*"
        };

        protected DocumentClient Client;
        protected string DatabaseId;

        protected static JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (_serializerSetting != null)
                    return _serializerSetting;

                _serializerSetting = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                _serializerSetting.Converters.Add(new StringEnumConverter());

                return _serializerSetting;
            }

        }

        protected DocumentDbProviderBase(DocumentClient client, string databaseId)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
        }

        public async Task InitAsync(DocumentDbEventStoreConfig config)
        {
            await CreateDatabaseIfNotExistsAsync().ConfigureAwait(false);

            foreach (var c in config.AggregateConfig)
            {
                await CreateAggregateCollectionIfNotExistsAsync(c.AggregateType, c.OfferThroughput).ConfigureAwait(false);

                await CreateSnapShotCollectionIfNotExistsAsync(c.AggregateType, c.SnapshotOfferThroughput).ConfigureAwait(false);
            }
        }

        protected static string GetClrTypeName(object item)
        {
            return item.GetType() + "," + item.GetType().Assembly.GetName().Name;
        }

        protected static string SnapshotCollectionName(Type aggregateType)
        {
            return $"{aggregateType.Name}-snapshot";
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await Client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId))
                    .ConfigureAwait(false);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await Client.CreateDatabaseAsync(new Database { Id = DatabaseId }).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }

        private Task CreateAggregateCollectionIfNotExistsAsync(Type aggregateType, int throughput)
        {
            return CreateCollectionAsync(aggregateType.Name, throughput, PartitionKey, ExcludePaths);
        }

        private Task CreateSnapShotCollectionIfNotExistsAsync(Type aggregateType, int throughput)
        {
            return CreateCollectionAsync(SnapshotCollectionName(aggregateType), throughput, PartitionKey, ExcludePaths);
        }

        private async Task CreateCollectionAsync(string collectionName, int throughput, string partitionKey, IEnumerable<string> excludePaths)
        {
            try
            {
                await Client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, collectionName))
                    .ConfigureAwait(false);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //todo pass in index policy
                    var collection = new DocumentCollection
                    {
                        Id = collectionName
                    };

                    collection.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });

                    foreach (var path in excludePaths)
                    {
                        collection.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath
                        {
                            Path = path
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(partitionKey))
                    {
                        collection.PartitionKey.Paths.Add(partitionKey);
                    }

                    await Client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        collection,
                        new RequestOptions { OfferThroughput = throughput }).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}