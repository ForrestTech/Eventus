namespace Eventus.SqlServer
{
    using Dapper;
    using Domain;
    using Events;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Transactions;

    public class SqlServerEventStorageProvider : SqlServerProviderBase, IEventStorageProvider
    {
        private readonly IDatabaseConnectionLogger _loggedConnection;

        public SqlServerEventStorageProvider(IDatabaseConnectionLogger loggedConnection, 
            EventusSqlServerOptions options) : base(options)
        {
            _loggedConnection = loggedConnection;
        }

        public async Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int offSet,
            int count)
        {
            var connection = await GetOpenConnectionAsync();

            await using (connection)
            {
                var sql =
                    $"Select top {count} * from {TableName(aggregateType)} where AggregateId = @aggregateId and AggregateVersion >= @start order by AggregateVersion";
                var events = await _loggedConnection.QueryAsync<SqlAggregateEvent>(connection, sql, new {aggregateId, start = offSet, count});

                var result = events.Select(DeserializeEvent);
                return result;
            }
        }

        public async Task<IEvent?> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            var connection = await GetOpenConnectionAsync();

            await using (connection)
            {
                var sql =
                    $"Select top 1 * from {TableName(aggregateType)} where AggregateId = @aggregateId order by AggregateVersion desc";
                var result = await _loggedConnection.QueryAsync<SqlAggregateEvent>(connection, sql, new {aggregateId});

                var @event = result.SingleOrDefault();

                return @event == null ? null : DeserializeEvent(@event);
            }
        }

        public async Task CommitChangesAsync(Aggregate aggregate)
        {
            var events = aggregate.GetUncommittedChanges();

            if (events.Any())
            {
                var committed = aggregate.LastCommittedVersion;

                var connection = await GetOpenConnectionAsync();

                await using (connection)
                {
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        foreach (var @event in events)
                        {
                            committed++;
                            var sql =
                                $"insert into {TableName(aggregate.GetType())} (Id, AggregateId, TargetVersion, ClrType, AggregateVersion, TimeStamp, Data) values (@Id, @AggregateId, @TargetVersion, @ClrType, @AggregateVersion, @TimeStamp, @Data)";
                            await _loggedConnection.ExecuteAsync(connection, sql,
                                new
                                {
                                    Id = @event.CorrelationId,
                                    @event.AggregateId,
                                    @event.TargetVersion,
                                    AggregateVersion = committed,
                                    ClrType = GetClrTypeName(@event),
                                    TimeStamp = @event.EventCommittedTimestamp,
                                    Data = SerializeEvent(@event)
                                });
                        }

                        transactionScope.Complete();
                    }
                }
            }
        }

        private static IEvent DeserializeEvent(SqlAggregateEvent returnedAggregateAggregateEvent)
        {
            var returnType = Type.GetType(returnedAggregateAggregateEvent.ClrType);

            var deserialize = JsonSerializer.Deserialize(returnedAggregateAggregateEvent.Data, returnType ?? throw new InvalidOperationException(), JsonSerializerOptions);

            return (Event)deserialize!;
        }

        private static string SerializeEvent(IEvent @event)
        {
            var serialized = JsonSerializer.Serialize(@event, @event.GetType(),  JsonSerializerOptions);
            return serialized;
        }
    }
}