using System;
using System.Threading.Tasks;
using Eventus.Storage;

namespace Eventus.SqlServer
{
    public class SqlServerSnapshotStorageProvider : ISnapshotStorageProvider
    {
        private readonly string _connectionString;

        public SqlServerSnapshotStorageProvider(string connectionString, int snapshotFrequency)
        {
            _connectionString = connectionString;
            SnapshotFrequency = snapshotFrequency;
        }

        public int SnapshotFrequency { get; }

        public Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            return Task.FromResult<Snapshot>(null);
        }

        public Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            return Task.FromResult<Snapshot>(null);
        }

        public Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            return Task.CompletedTask;
        }
    }
}