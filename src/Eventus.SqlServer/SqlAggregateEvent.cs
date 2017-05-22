namespace Eventus.SqlServer
{
    public class SqlAggregateEvent : SqlEventBase
    {
        public int TargetVersion { get; set; }
    }
}