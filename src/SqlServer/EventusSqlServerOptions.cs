namespace Eventus.SqlServer
{
    public class EventusSqlServerOptions
    {
        public EventusSqlServerOptions(string connectionString)
        {
            ConnectionString = connectionString;
            Schema = "dbo";
        }

        public string ConnectionString { get; set; }
        
        public string Schema { get; set; }
    }
}