namespace Eventus.SqlServer
{
    using System;

    public class AggregateConfig
    {
        public AggregateConfig(Type aggregateType)
        {
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        }

        public Type AggregateType { get; }
    }
}