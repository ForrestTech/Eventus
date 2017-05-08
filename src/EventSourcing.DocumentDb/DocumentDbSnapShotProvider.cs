using System;
using System.Threading.Tasks;
using EventSourcing.Repository;
using Microsoft.Azure.Documents.Client;

namespace EventSourcing.DocumentDb
{
    public class DocumentDbSnapShotProvider : ISnapshotStorageProvider
    {
        private readonly DocumentClient _client;

        public DocumentDbSnapShotProvider(DocumentClient client)
        {
            _client = client;
        }

        public int SnapshotFrequency => 100;

        public Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            throw new NotImplementedException();
        }

        public Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            throw new NotImplementedException();
        }
    }
}