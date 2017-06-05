using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Eventus.Domain;
using Eventus.Events;
using Eventus.Storage;
using Newtonsoft.Json;

namespace Eventus.SqlServer
{
    public class SqlServerEventStorageProvider : SqlServerProviderBase, IEventStorageProvider
    {
        public SqlServerEventStorageProvider(string connectionString) : base(connectionString)
        {
        }

        public async Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start = 0, int count = int.MaxValue)
        {
            var connection = await GetOpenConnectionAsync().ConfigureAwait(false);

            using (connection)
            {
                var sql = $"Select top {count} * from {TableName(aggregateType)} where AggregateId = @aggregateId and AggregateVersion >= @start order by AggregateVersion";
                var events = await connection.QueryAsync<SqlAggregateEvent>(sql, new { aggregateId, start, count })
                    .ConfigureAwait(false);

                var result = events.Select(DeserializeEvent);
                return result;
            }
        }

        public async Task<IEvent> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            var connection = await GetOpenConnectionAsync().ConfigureAwait(false);

            using (connection)
            {
                var sql = $"Select top 1 * from {TableName(aggregateType)} where AggregateId = @aggregateId order by AggregateVersion desc";
                var result = await connection.QueryAsync<SqlAggregateEvent>(sql, new { aggregateId })
                    .ConfigureAwait(false);

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

                var connection = await GetOpenConnectionAsync().ConfigureAwait(false);

                using (connection)
                {
                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        foreach (var @event in events)
                        {
                            committed++;
                            var sql = $"insert into {TableName(aggregate.GetType())} (Id, AggregateId, TargetVersion, ClrType, AggregateVersion, TimeStamp, Data) values (@Id, @AggregateId, @TargetVersion, @ClrType, @AggregateVersion, @TimeStamp, @Data)";
                            await connection.ExecuteAsync(sql, new
                            {
                                Id = @event.CorrelationId,
                                @event.AggregateId,
                                @event.TargetVersion,
                                AggregateVersion = committed,
                                ClrType = GetClrTypeName(@event),
                                TimeStamp = @event.EventCommittedTimestamp,
                                Data = SerializeEvent(@event)
                            }).ConfigureAwait(false);
                        }
                        transactionScope.Complete();
                    }
                }
            }
        }

        private static IEvent DeserializeEvent(SqlAggregateEvent returnedAggregateAggregateEvent)
        {
            var returnType = Type.GetType(returnedAggregateAggregateEvent.ClrType);

            return (Event)JsonConvert.DeserializeObject(returnedAggregateAggregateEvent.Data, returnType, SerializerSettings);
        }

        private static string SerializeEvent(IEvent @event)
        {
            return JsonConvert.SerializeObject(@event, SerializerSettings);
        }
    }
}
