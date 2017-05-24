using System;

namespace Eventus.DocumentDb.Config
{
    public class AggregateConfig
    {
        public Type AggregateType { get; set; }
        public int SnapshotOfferThroughput { get; set; }
        public int OfferThroughput { get; set; }
    }
}