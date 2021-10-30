namespace Eventus.Configuration
{
    using System;

    public class AggregateConfig
    {
        public AggregateConfig(Type type)
        {
            Type = type;
        }
        
        public Type Type { get; set; }

        public bool SnapshotDisabled { get; set; } = false;
        
        public int SnapshotFrequency { get; set; }
    }
}