using Newtonsoft.Json;

namespace Eventus.DocumentDb
{
    public class DocumentDbAggregateEvent : DocumentDbEventBase
    {
        [JsonProperty("targetVersion")]
        public int TargetVersion { get; set; }
    }
}