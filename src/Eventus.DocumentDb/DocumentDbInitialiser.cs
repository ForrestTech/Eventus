using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eventus.Config;
using Eventus.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Eventus.DocumentDb
{
    public class DocumentDbInitialiser : IInitialiseStorageProvider
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;
        private const string PartitionKey = "/aggregateId";
        private static readonly List<string> ExcludePaths = new List<string>
        {
            "/data/*",
            "/clrType/*",
            "/targetVersion/*",
            "/timestamp/*"
        };

        public DocumentDbInitialiser(DocumentClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        public async Task InitAsync(ProviderConfig config)
        {
            await CreateDatabaseIfNotExistsAsync().ConfigureAwait(false);

            foreach (var c in config.Aggregates)
            {
                await CreateAggregateCollectionIfNotExistsAsync(c).ConfigureAwait(false);

                await CreateSnapShotCollectionIfNotExistsAsync(c).ConfigureAwait(false);
            }
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_databaseId))
                    .ConfigureAwait(false);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _client.CreateDatabaseAsync(new Database { Id = _databaseId }).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }

        private Task CreateAggregateCollectionIfNotExistsAsync(AggregateConfig config)
        {
            return CreateCollectionAsync(config.AggregateType.Name, config.Settings.OfferThroughput, PartitionKey, ExcludePaths);
        }

        private Task CreateSnapShotCollectionIfNotExistsAsync(AggregateConfig config)
        {
            return CreateCollectionAsync(SnapshotCollectionName(config.AggregateType), config.Settings.SnapshotOfferThroughput, PartitionKey, ExcludePaths);
        }

        private async Task CreateCollectionAsync(string collectionName, int throughput, string partitionKey, IEnumerable<string> excludePaths)
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionName))
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

                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_databaseId),
                        collection,
                        new RequestOptions { OfferThroughput = throughput }).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }

        protected static string SnapshotCollectionName(Type aggregateType)
        {
            return $"{aggregateType.Name}-snapshot";
        }
    }
}