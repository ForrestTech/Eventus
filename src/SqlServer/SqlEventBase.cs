namespace Eventus.SqlServer
{
    using System;
    
    public class SqlEventBase
    {
        public Guid Id { get; set; }

        public Guid AggregateId { get; set; }

        public int Version { get; set; }

        public string ClrType { get; set; } = string.Empty;

        public DateTime TimeStamp { get; set; }

        public string Data { get; set; } = string.Empty;
    }
}