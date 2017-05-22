using System.Collections.Generic;

namespace Eventus.Config
{
    public class ProviderConfig
    {
        public ProviderConfig(IEnumerable<AggregateConfig> aggregates)
        {
            Aggregates = aggregates;
        }

        public IEnumerable<AggregateConfig> Aggregates { get; set; }
    }
}