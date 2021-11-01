namespace Eventus.CosmosDB
{
    using Newtonsoft.Json;
    using System;

    public abstract class CosmosDBEventBase
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        
        public Guid AggregateId { get; set; }
        public string ClrType { get; set; } = string.Empty;

        public int Version { get; set; }

        public DateTime Timestamp { get; set; }

        public string Data { get; set; } = string.Empty;
    }
}