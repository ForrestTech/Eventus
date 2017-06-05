using System;

namespace Eventus.SqlServer.Config
{
    public class AggregateConfig
    {
        public AggregateConfig(Type aggregateType)
        {
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        }

        public Type AggregateType { get; }
    }
}