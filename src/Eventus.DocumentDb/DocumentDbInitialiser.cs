using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Eventus.DocumentDb.Config;
using Eventus.Domain;
using Eventus.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Eventus.DocumentDb
{
    public class DocumentDbInitialiser : IStorageProviderInitialiser
    {
        private readonly DocumentClient _client;
        private readonly DocumentDbConfig _config;

        public DocumentDbInitialiser(DocumentClient client, DocumentDbConfig config)
        {
            _client = client;
            _config = config;
        }

        public async Task InitAsync()
        {

            //todo find a way to merge custom code config with default config
            var aggregateTypes = await DetectAggregatesAsync().ConfigureAwait(false);

            var aggregates = await BuildAggregateConfigsAsync(aggregateTypes).ConfigureAwait(false);

            await CreateDatabaseIfNotExistsAsync().ConfigureAwait(false);

            foreach (var c in aggregates)
            {
                await CreateAggregateCollectionIfNotExistsAsync(c).ConfigureAwait(false);

                await CreateSnapShotCollectionIfNotExistsAsync(c).ConfigureAwait(false);
            }
        }

        protected virtual Task<IEnumerable<Type>> DetectAggregatesAsync()
        {
            var aggregateTypes = AggregateHelper.GetAggregateTypes();

            return Task.FromResult(aggregateTypes);
        }

        protected virtual Task<IEnumerable<AggregateConfig>> BuildAggregateConfigsAsync(IEnumerable<Type> aggregateTypes)
        {
            var aggregateConfigs = aggregateTypes.Select(t => new AggregateConfig(t, _config.DefaultThroughput, _config.DefaultSnapshotThroughput));

            return Task.FromResult(aggregateConfigs);
        }

        protected virtual async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_config.DatabaseId))
                    .ConfigureAwait(false);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _client.CreateDatabaseAsync(new Database { Id = _config.DatabaseId }).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }

        protected virtual Task CreateAggregateCollectionIfNotExistsAsync(AggregateConfig config)
        {
            return CreateCollectionAsync(config.AggregateType.Name, config.OfferThroughput);
        }

        protected virtual Task CreateSnapShotCollectionIfNotExistsAsync(AggregateConfig config)
        {
            return CreateCollectionAsync(SnapshotCollectionName(config.AggregateType), config.SnapshotOfferThroughput);
        }

        protected virtual async Task CreateCollectionAsync(string collectionName, int throughput)
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_config.DatabaseId, collectionName))
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

                    foreach (var path in _config.ExcludePaths )
                    {
                        collection.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath
                        {
                            Path = path
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(_config.PartitionKey))
                    {
                        collection.PartitionKey.Paths.Add(_config.PartitionKey);
                    }

                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_config.DatabaseId),
                        collection,
                        new RequestOptions { OfferThroughput = throughput }).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }

        protected virtual string SnapshotCollectionName(Type aggregateType)
        {
            return $"{aggregateType.Name}-snapshot";
        }
    }
}