namespace Eventus.SqlServer
{
    using Dapper;
    using Storage;
    using System.Linq;
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class SqlServerSnapshotStorageProvider : SqlServerProviderBase, ISnapshotStorageProvider
    {
        public SqlServerSnapshotStorageProvider(EventusSqlServerOptions options) : base(options)
        {
        }

        public async Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId, int version)
        {
            var connection = await GetOpenConnectionAsync();

            await using (connection)
            {
                var sql =
                    $"Select * from {SnapshotTableName(aggregateType)} where AggregateId = @aggregateId and AggregateVersion  @version";
                var events = await connection.QueryAsync<SqlSnapshot>(sql, new {aggregateId, version});

                var snapshot = events.SingleOrDefault();
                if (snapshot == null)
                    return null;

                var result = DeserializeSnapshot(snapshot);
                return result;
            }
        }

        public async Task<Snapshot?> GetSnapshotAsync(Type aggregateType, Guid aggregateId)
        {
            var connection = await GetOpenConnectionAsync();

            await using (connection)
            {
                var sql =
                    $"Select top 1 * from {SnapshotTableName(aggregateType)} where AggregateId = @aggregateId order by AggregateVersion desc";
                var result = await connection.QueryAsync<SqlSnapshot>(sql, new {aggregateId});

                var @event = result.SingleOrDefault();

                return @event == null ? null : DeserializeSnapshot(@event);
            }
        }

        public async Task SaveSnapshotAsync(Type aggregateType, Snapshot snapshot)
        {
            var connection = await GetOpenConnectionAsync();

            await using (connection)
            {
                var sql =
                    $"insert into {SnapshotTableName(aggregateType)} (Id, AggregateId, AggregateVersion, ClrType, TimeStamp, Data) values (@Id, @AggregateId, @AggregateVersion, @ClrType, @TimeStamp, @Data)";

                await connection.ExecuteAsync(sql,
                    new
                    {
                        snapshot.Id,
                        snapshot.AggregateId,
                        AggregateVersion = snapshot.Version,
                        ClrType = GetClrTypeName(snapshot),
                        TimeStamp = Clock.Now(),
                        Data = SerializeSnapshot(snapshot)
                    });
            }
        }

        private static string SerializeSnapshot(Snapshot snapshot)
        {
            var serialized = JsonSerializer.Serialize(snapshot, snapshot.GetType(), JsonSerializerOptions);
            return serialized;
        }

        private static Snapshot DeserializeSnapshot(SqlEventBase item)
        {
            var returnType = Type.GetType(item.ClrType);

            var deserialized = JsonSerializer.Deserialize(item.Data,
                returnType ?? throw new InvalidOperationException(), JsonSerializerOptions);

            return (Snapshot)deserialized!;
        }
    }
}