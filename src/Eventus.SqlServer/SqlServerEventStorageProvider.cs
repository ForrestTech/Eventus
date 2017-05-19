using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Eventus.Domain;
using Eventus.Events;
using Eventus.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Eventus.SqlServer
{
    public class SqlServerEventStorageProvider : IEventStorageProvider
    {
        private static JsonSerializerSettings _serializerSetting;

        private readonly SqlConnection _connection;

        public SqlServerEventStorageProvider(SqlConnection connection)
        {
            _connection = connection;
        }

        protected static JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (_serializerSetting != null)
                    return _serializerSetting;

                _serializerSetting = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                _serializerSetting.Converters.Add(new StringEnumConverter());

                return _serializerSetting;
            }
        }

        public void Init()
        {
            //implement logic to create a table for each aggregate

            //loop through aggregates provided

            //check table exists

            //create aggregate table

            //create snapshot table if needed
        }

        public async Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int start = 0, int count = int.MaxValue)
        {
            var sql = $"Select top {count} * from {TableName(aggregateType)} where AggregateId = @aggregateId and AggregateVersion >= @start order by AggregateVersion";
            var events = await _connection.QueryAsync<AggregateEvent>(sql, new { aggregateId, start, count })
                .ConfigureAwait(false);

            var result = events.Select(DeserializeEvent);

            return result;
        }

        public async Task<IEvent> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            var sql = $"Select top 1 * from {TableName(aggregateType)} where AggregateId = @aggregateId order by desc AggregateVersion";
            var result = await _connection.QueryAsync<AggregateEvent>(sql, new { aggregateId })
                .ConfigureAwait(false);

            var @event = result.SingleOrDefault();

            return @event == null ? null : DeserializeEvent(@event);
        }

        public async Task CommitChangesAsync(Aggregate aggregate)
        {
            var events = aggregate.GetUncommittedChanges();

            if (events.Any())
            {
                var committed = aggregate.LastCommittedVersion;

                using (var transactionScope = new TransactionScope())
                {
                    foreach (var @event in events)
                    {
                        committed++;
                        //todo move to bulk insert
                        var sql = $"insert into {TableName(aggregate.GetType())} (Id, AggregateId, TargetVersion, ClrType, AggregateVersion, TimeStamp, Data) values (@Id, @AggregateId, @TargetVersion, @ClrType, @AggregateVersion, @TimeStamp, @Data)";
                        await _connection.ExecuteAsync(sql, new
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

        private static IEvent DeserializeEvent(AggregateEvent returnedAggregateEvent)
        {
            var returnType = Type.GetType(returnedAggregateEvent.ClrType);

            return (Event)JsonConvert.DeserializeObject(returnedAggregateEvent.Data, returnType, SerializerSettings);
        }

        private static string SerializeEvent(IEvent @event)
        {
            return JsonConvert.SerializeObject(@event, SerializerSettings);
        }

        protected static string GetClrTypeName(object item)
        {
            return item.GetType() + "," + item.GetType().Assembly.GetName().Name;
        }

        private string TableName(Type aggregateType)
        {
            return aggregateType.Name;
        }
    }

    public class AggregateEvent
    {
        public Guid Id { get; set; }

        public Guid AggregateId { get; set; }

        public int TargetVersion { get; set; }

        public int Version { get; set; }

        public string ClrType { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Data { get; set; }
    }
}
