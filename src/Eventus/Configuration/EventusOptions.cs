namespace Eventus.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EventusOptions
    {
        /// <summary>
        /// Globally disable snapshotting.  Even if aggregates are marked as Snapshottable this will disable the feature all together
        /// </summary>
        public bool SnapshottingEnabled { get; set; } = true;

        /// <summary>
        /// The Global snapshot frequency setting.  If not aggregate specific settings are set then this value will be used
        /// </summary>
        public int SnapshotFrequency { get; set; } = 10;

        public List<AggregateConfig> AggregateConfigs { get; set; } = new();

        public bool GetSnapshotEnabled(Type aggregateType)
        {
            var agg = GetAggregateConfig(aggregateType);
            return agg?.SnapshotDisabled ?? SnapshottingEnabled;
        }

        public int GetSnapshotFrequency(Type aggregateType)
        {
            var agg = GetAggregateConfig(aggregateType);
            return agg?.SnapshotFrequency ?? SnapshotFrequency;
        }

        private AggregateConfig? GetAggregateConfig(Type aggregateType)
        {
            return AggregateConfigs.SingleOrDefault(x => x.Type == aggregateType);
        }
    }
}