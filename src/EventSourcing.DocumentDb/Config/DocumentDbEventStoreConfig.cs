using System.Collections.Generic;

namespace EventSourcing.DocumentDb.Config
{
    public class DocumentDbEventStoreConfig
    {
        public IEnumerable<AggregateConfig> AggregateConfig { get; set; }
    }
}