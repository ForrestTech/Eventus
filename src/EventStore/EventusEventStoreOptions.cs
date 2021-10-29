namespace Eventus.EventStore
{
    public class EventusEventStoreOptions
    {
        public EventusEventStoreOptions(string connectionString)
        {
            ConnectionString = connectionString;
            StreamPrefix = "Eventus";
        }

        public string ConnectionString { get; }
        
        public string StreamPrefix { get; set; }
    }
}