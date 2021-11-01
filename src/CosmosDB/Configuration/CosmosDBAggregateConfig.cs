namespace Eventus.CosmosDB.Configuration
{
    using System;

    public class CosmosDBAggregateConfig
    {
        public CosmosDBAggregateConfig(Type aggregateType)
        {
            Type = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        }

        public Type Type { get; }

        public int? SnapshotContainerThroughput  { get; set; } = null;

        public int? AggregateContainerThroughput { get; set; } = null;
        
        
        public int? SnapshotContainerAutoScaleThroughput { get; set; }


        public int? AggregateContainerAutoScaleThroughput { get; set; } = null;
    }
}