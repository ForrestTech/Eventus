using System.Collections.Generic;

namespace EventSourcing.DocumentDb
{
    public class DocumentDbEventStoreConfig
    {
        public IEnumerable<AggregateConfig> AggregateConfig { get; set; }
    }
}