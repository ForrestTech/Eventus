using System;

namespace EventSourcing.DocumentDb
{
    public class AggregateConfig
    {
        public Type AggregateType { get; set; }

        public int OfferThroughput { get; set; }
    }
}