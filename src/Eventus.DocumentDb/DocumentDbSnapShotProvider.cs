using System;
using System.Linq;
using System.Threading.Tasks;
using Eventus.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace Eventus.DocumentDb
{
    public class DocumentDbSnapShotProvider : DocumentDbProviderBase, ISnapshotStorageProvider
    {
        public DocumentDbSnapShotProvider(DocumentClient client, string databaseId, int snapshotFrequency) : base(client, databaseId)
        {
            SnapshotFrequency = snapshotFrequency;
        }

        public int SnapshotFrequency { get; }

        public async Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            try
            {
                var query = Client.CreateDocumentQuery<DocumentDbSnapshot>(
                        SnapshotCollectionUri(aggregateType),
                        new FeedOptions { MaxItemCount = -1 })
                    .Where(x => x.AggregateId == aggregateId && x.Version == version)
                    .AsDocumentQuery();

                var result = await query.ExecuteNextAsync<DocumentDbSnapshot>()
                    .ConfigureAwait(false);

                var item = result.SingleOrDefault();

                return item == null ? null : DeserializeSnapshot(item);
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

        public async Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            try
            {
                var query = Client.CreateDocumentQuery<DocumentDbSnapshot>(
                        SnapshotCollectionUri(aggregateType),
                        new FeedOptions { MaxItemCount = -1 })
                    .Where(x => x.AggregateId == aggregateId)
                    .OrderByDescending(x => x.Version)
                    .Take(1)
                    .AsDocumentQuery();

                var result = await query.ExecuteNextAsync<DocumentDbSnapshot>()
                    .ConfigureAwait(false);

                var item = result.SingleOrDefault();

                return item == null ? null : DeserializeSnapshot(item);
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

        public Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            var documentDbSnapshot = CreateSnapshotEvent(snapshot);
            return Client.CreateDocumentAsync(SnapshotCollectionUri(aggregateType), documentDbSnapshot);
        }

        private static DocumentDbSnapshot CreateSnapshotEvent(Snapshot snapshot)
        {
            return new DocumentDbSnapshot
            {
                Id = snapshot.Id,
                AggregateId = snapshot.AggregateId,
                ClrType = GetClrTypeName(snapshot),
                Version = snapshot.Version,
                Timestamp = Clock.Now(),
                Data = SerializeSnapshot(snapshot)
            };
        }

        private static string SerializeSnapshot(Snapshot snapshot)
        {
            var serialized = JsonConvert.SerializeObject(snapshot, SerializerSettings);
            return serialized;
        }

        private static Snapshot DeserializeSnapshot(DocumentDbEventBase item)
        {
            var returnType = Type.GetType(item.ClrType);

            var deserialized = JsonConvert.DeserializeObject(item.Data, returnType, SerializerSettings);

            return (Snapshot)deserialized;
        }

        protected Uri EventCollectionUri(Type aggregateType)
        {
            return UriFactory.CreateDocumentCollectionUri(DatabaseId, aggregateType.Name);
        }

        protected Uri SnapshotCollectionUri(Type aggregateType)
        {
            return UriFactory.CreateDocumentCollectionUri(DatabaseId, SnapshotCollectionName(aggregateType));
        }
    }
}