using System.Collections.Generic;

namespace Eventus.DocumentDb.Config
{
    public class DocumentDbEventStoreConfig
    {
        public IEnumerable<AggregateConfig> AggregateConfig { get; set; }
    }
}