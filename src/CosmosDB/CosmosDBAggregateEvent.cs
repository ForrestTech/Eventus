namespace Eventus.CosmosDB
{
    public class CosmosDBAggregateEvent : CosmosDBEventBase
    {
        public int TargetVersion { get; set; }
    }
}