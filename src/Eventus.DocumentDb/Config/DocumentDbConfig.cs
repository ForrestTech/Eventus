using System.Collections.Generic;

namespace Eventus.DocumentDb.Config
{
    public class DocumentDbConfig
    {
        public IEnumerable<AggregateConfig> Aggregates { get; set; }
    }
}