using System;
using System.Collections.Generic;

namespace Eventus.SqlServer.Config
{
    public class SqlServerConfig
    {
        public SqlServerConfig(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            Schema = "dbo";
        }

        public string Schema { get; set; }

        public string ConnectionString { get; }
    }
}