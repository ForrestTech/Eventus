namespace Eventus.SqlServer
{
    public class CosmosDBAggregateEvent : CosmosDBEventBase
    {
        public int TargetVersion { get; set; }
    }
}