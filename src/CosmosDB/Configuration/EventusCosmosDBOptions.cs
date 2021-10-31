namespace Eventus.SqlServer.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EventusCosmosDBOptions
    {
        public EventusCosmosDBOptions(string databaseId)
        {
            DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
        }

        /// <summary>
        /// The id of the database in cosmos
        /// </summary>
        public string DatabaseId { get; }

        public string ApplicationName { get; set; } = "Eventus";

        public string PartitionKey { get; set; } = "/AggregateId";

        /// <summary>
        /// The database shared throughput if set a throughput will be placed on the whole cosmos DB database that can be shared by any containers
        /// </summary>
        public int? DatabaseSharedThroughPut { get; set; } = null;

        /// <summary>
        /// The default manual throughput for all aggregate containers
        /// </summary>
        public int? AggregateContainersThroughput { get; set; } = null;

        /// <summary>
        /// The default auto scale throughput for all aggregate containers.  This will override any manual throughput set
        /// </summary>
        public int? AggregateContainersAutoScaleThroughput { get; set; } = null;

        /// <summary>
        /// The default manual throughput for all snapshot containers
        /// </summary>
        public int? SnapshotContainersThroughput { get; set; } = null;

        /// <summary>
        /// The default auto scale throughput for all snapshot containers.  This will override any manual throughput set
        /// </summary>
        public int? SnapshotContainersAutoScaleThroughput { get; set; } = null;

        public List<string> ExcludePaths { get; set; } = new();

        /// <summary>
        /// Aggregate specific CosmosDB configuration that will override any global configuration 
        /// </summary>
        public List<CosmosDBAggregateConfig> AggregateConfigs { get; set; } = new();

        public int? GetSnapshotThroughput(Type aggregateType)
        {
            var agg = GetAggregateConfig(aggregateType);
            return agg?.SnapshotContainerThroughput ?? SnapshotContainersThroughput;
        }

        public int? GetAggregateThroughput(Type aggregateType)
        {
            var agg = GetAggregateConfig(aggregateType);
            return agg?.AggregateContainerThroughput ?? AggregateContainersThroughput;
        }

        public int? GetAggregateAutoScaleThroughput(Type aggregateType)
        {
            var agg = GetAggregateConfig(aggregateType);
            return agg?.AggregateContainerAutoScaleThroughput ?? AggregateContainersAutoScaleThroughput;
        }

        public int? GetSnapshotAutoScaleThroughput(Type aggregateType)
        {
            var agg = GetAggregateConfig(aggregateType);
            return agg?.SnapshotContainerAutoScaleThroughput ?? SnapshotContainersAutoScaleThroughput;
        }

        private CosmosDBAggregateConfig? GetAggregateConfig(Type aggregateType)
        {
            return AggregateConfigs.SingleOrDefault(x => x.Type == aggregateType);
        }
    }
}