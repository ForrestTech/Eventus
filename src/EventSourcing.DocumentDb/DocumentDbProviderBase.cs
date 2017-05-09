using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.DocumentDb.Config;
using EventSourcing.Repository;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace EventSourcing.DocumentDb
{
    public abstract class DocumentDbProviderBase
    {
        private const string PartitionKey = "/aggregateId";

        private static readonly List<string> ExcludePaths = new List<string>
        {
            "/data/*",
            "/clrType/*",
            "/targetVersion/*",
            "/timestamp/*"
        };


        protected DocumentClient Client;
        protected string DatabaseId;
        protected static JsonSerializerSettings SerializerSetting;

        protected DocumentDbProviderBase(DocumentClient client, string databaseId)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
        }

        public async Task InitAsync(DocumentDbEventStoreConfig config)
        {
            await CreateDatabaseIfNotExistsAsync();

            foreach (var c in config.AggregateConfig)
            {
                await CreateAggreateCollectionIfNotExistsAsync(c.AggregateType, c.OfferThroughput);

                await CreateSnapShotCollectionIfNotExistsAsync(c.AggregateType, c.SnapshotOfferThroughput);
            }
        }

        protected static JsonSerializerSettings SerializerSettings => SerializerSetting ?? (SerializerSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });

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
                await Client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await Client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateAggreateCollectionIfNotExistsAsync(Type aggregateType, int throughput)
        {
            await CreateCollection(aggregateType.Name, throughput, PartitionKey, ExcludePaths);
        }

        private async Task CreateSnapShotCollectionIfNotExistsAsync(Type aggregateType, int throughput)
        {
            await CreateCollection(SnapshotCollectionName(aggregateType), throughput, PartitionKey, ExcludePaths);
        }

        private async Task CreateCollection(string collectionName, int throughput, string partitionKey, IEnumerable<string> excludePaths)
        {
            try
            {
                await Client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, collectionName));
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
                        new RequestOptions { OfferThroughput = throughput });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}