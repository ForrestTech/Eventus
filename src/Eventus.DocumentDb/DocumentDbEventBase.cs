using System;
using Newtonsoft.Json;

namespace Eventus.DocumentDb
{
    public abstract class DocumentDbEventBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("aggregateId")]
        public Guid AggregateId { get; set; }

        [JsonProperty("clrType")]
        public string ClrType { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }
}