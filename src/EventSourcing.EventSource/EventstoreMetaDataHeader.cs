namespace EventSourcing.EventSource
{
    public class EventstoreMetaDataHeader
    {
        public string ClrType { get; set; }
        public int CommitNumber { get; set; }
    }
}