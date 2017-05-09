using Newtonsoft.Json;

namespace EventSourcing.DocumentDb
{
    public class DocumentDbAggregateEvent : DocumentDbEventBase
    {
        [JsonProperty("targetVersion")]
        public int TargetVersion { get; set; }
    }
}