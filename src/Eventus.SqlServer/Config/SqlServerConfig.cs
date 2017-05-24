using System.Collections.Generic;

namespace Eventus.SqlServer.Config
{
    public class SqlServerConfig
    {
        public IEnumerable<AggregateConfig> Aggregates { get; set; }
    }
}