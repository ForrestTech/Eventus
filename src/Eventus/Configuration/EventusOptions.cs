namespace Eventus.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class EventusOptions
    {
        /// <summary>
        /// Enabled timing of event storage requests 
        /// </summary>
        public bool DiagnosticTimingEnabled { get; set; } = false;
        
        /// <summary>
        /// Wraps all calls to repository in a decorator that logs requests 
        /// </summary>
        public bool RepositoryLoggingEnabled { get; set; } = false;

        /// <summary>
        /// Globally disable snapshotting.  Even if aggregates are marked as Snapshottable this will disable the feature all together
        /// </summary>
        public bool SnapshottingEnabled { get; set; } = true;

        /// <summary>
        /// The Global snapshot frequency setting.  If not aggregate specific settings are set then this value will be used
        /// </summary>
        public int SnapshotFrequency { get; set; } = 10;

        /// <summary>
        /// Aggregate specific config settings
        /// </summary>
        public List<AggregateConfig> AggregateConfigs { get; } = new();

        /// <summary>
        /// Global default JsonSerializerOptions used by storage providers when storing event and snapshot data for aggregates. 
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = {new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)}
        };

        /// <summary>
        /// Are snapshots enabled for the supplied aggregate type.  Gets the aggregate specific setting if one exists and the global setting if not.  
        /// </summary>
        public bool GetSnapshotEnabled(Type aggregateType)
        {
            var agg = GetAggregateConfig(aggregateType);
            return agg?.SnapshotDisabled ?? SnapshottingEnabled;
        }

        /// <summary>
        /// Gets the snapshot frequency for the supplied aggregate type.  Gets the aggregate specific setting if one exists and the global setting if not.
        /// </summary>
        /// <param name="aggregateType"></param>
        /// <returns></returns>
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
