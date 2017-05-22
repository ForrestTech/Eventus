using System;

namespace Eventus.SqlServer
{
    public class SqlEventBase
    {
        public Guid Id { get; set; }

        public Guid AggregateId { get; set; }

        public int Version { get; set; }

        public string ClrType { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Data { get; set; }
    }
}