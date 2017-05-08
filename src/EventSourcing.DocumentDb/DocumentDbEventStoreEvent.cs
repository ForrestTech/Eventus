using System;
using Newtonsoft.Json;

namespace EventSourcing.DocumentDb
{
    public class DocumentDbEventStoreEvent
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("aggregateId")]
        public Guid AggregateId { get; set; }

        [JsonProperty("clrType")]
        public string ClrType { get; set; }

        [JsonProperty("commited")]
        public int Commited { get; set; }
        
        [JsonProperty("eventTimestamp")]
        public DateTime EventTimestamp { get; set; }

        [JsonProperty("eventData")]
        public string EventData { get; set; }
    }
}