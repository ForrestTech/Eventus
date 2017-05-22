using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Eventus.Storage;
using Newtonsoft.Json;

namespace Eventus.SqlServer
{
    public class SqlServerSnapshotStorageProvider : SqlServerProviderBase, ISnapshotStorageProvider
    {
        public SqlServerSnapshotStorageProvider(string connectionString, int snapshotFrequency) : base(connectionString)
        {
            SnapshotFrequency = snapshotFrequency;
        }

        public int SnapshotFrequency { get; }

        public async Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            var connection = await GetOpenConnectionAsync().ConfigureAwait(false);

            using (connection)
            {
                var sql = $"Select * from {SnapshotTableName(aggregateType)} where AggregateId = @aggregateId and AggregateVersion  @version";
                var events = await connection.QueryAsync<SqlSnapshot>(sql, new { aggregateId, version })
                    .ConfigureAwait(false);

                var snapshot = events.SingleOrDefault();
                if (snapshot == null)
                    return null;

                var result = DeserializeSnapshot(snapshot);
                return result;
            }
        }

        public async Task<Snapshot> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            var connection = await GetOpenConnectionAsync().ConfigureAwait(false);

            using (connection)
            {
                var sql = $"Select top 1 * from {SnapshotTableName(aggregateType)} where AggregateId = @aggregateId order by AggregateVersion desc";
                var result = await connection.QueryAsync<SqlSnapshot>(sql, new { aggregateId })
                    .ConfigureAwait(false);

                var @event = result.SingleOrDefault();

                return @event == null ? null : DeserializeSnapshot(@event);
            }
        }

        public async Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            var connection = await GetOpenConnectionAsync().ConfigureAwait(false);

            using (connection)
            {
                var sql = $"insert into {SnapshotTableName(aggregateType)} (Id, AggregateId, AggregateVersion, ClrType, TimeStamp, Data) values (@Id, @AggregateId, @AggregateVersion, @ClrType, @TimeStamp, @Data)";

                await connection.ExecuteAsync(sql, new
                {
                    snapshot.Id,
                    snapshot.AggregateId,
                    AggregateVersion = snapshot.Version,
                    ClrType = GetClrTypeName(snapshot),
                    TimeStamp = Clock.Now(),
                    Data = SerializeSnapshot(snapshot)
                }).ConfigureAwait(false);
            }
        }

        private static string SnapshotTableName(Type aggregateType)
        {
            return $"{aggregateType.Name}_Snapshot";
        }

        private static string SerializeSnapshot(Snapshot snapshot)
        {
            var serialized = JsonConvert.SerializeObject(snapshot, SerializerSettings);
            return serialized;
        }

        private static Snapshot DeserializeSnapshot(SqlEventBase item)
        {
            var returnType = Type.GetType(item.ClrType);

            var deserialized = JsonConvert.DeserializeObject(item.Data, returnType, SerializerSettings);

            return (Snapshot)deserialized;
        }

    }
}
 