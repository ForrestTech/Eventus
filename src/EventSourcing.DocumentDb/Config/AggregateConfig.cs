using System;

namespace EventSourcing.DocumentDb.Config
{
    public class AggregateConfig
    {
        public AggregateConfig()
        {
            OfferThroughput = 400;
            SnapshotOfferThroughput = 400;
        }

        public Type AggregateType { get; set; }

        public int OfferThroughput { get; set; }

        public int SnapshotOfferThroughput { get; set; }
    }
}