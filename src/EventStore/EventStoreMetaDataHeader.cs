namespace Eventus.EventStore
{
    public class EventStoreMetaDataHeader
    {
        public string ClrType { get; set; } = string.Empty;
        public int CommitNumber { get; set; } 
    }
}