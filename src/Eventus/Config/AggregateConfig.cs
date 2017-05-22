using System;

namespace Eventus.Config
{
    public class AggregateConfig
    {
        public AggregateConfig(Type aggregateType)
        {
            AggregateType = aggregateType;
        }

        public Type AggregateType { get; set; }

        public dynamic Settings { get; set; }
    }
}