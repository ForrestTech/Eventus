namespace Eventus.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// A simple in memory snap shot storage provider that can be used for development and testing
    /// </summary>
    public class InMemorySnapshotStorageProvider : ISnapshotStorageProvider
    {
        private static readonly Dictionary<Guid, Snapshot> Storage = new();
        
        public int SnapshotFrequency => 5;
        
        public Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            //you always get only one
            var snapshot = Storage.GetValueOrDefault(aggregateId);

            return (snapshot != null ? 
                Task.FromResult(snapshot) : 
                Task.FromResult<Snapshot?>(null)!)!;
        }

        public Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            var snapshot = Storage.GetValueOrDefault(aggregateId);

            return (snapshot != null ? 
                Task.FromResult(snapshot) : 
                Task.FromResult<Snapshot?>(null)!)!;
        }

        public Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            var key = snapshot.AggregateId;

            Storage[key] = snapshot;
            
            return Task.CompletedTask;
        }
    }
}