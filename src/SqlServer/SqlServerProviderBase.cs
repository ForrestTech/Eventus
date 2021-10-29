namespace Eventus.SqlServer
{
    using Microsoft.Data.SqlClient;
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public abstract class SqlServerProviderBase
    {
        protected readonly EventusSqlServerOptions Options;

        protected static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        protected SqlServerProviderBase(EventusSqlServerOptions options)
        {
            Options = options;
        }

        protected async Task<SqlConnection> GetOpenConnectionAsync()
        {
            var connection = new SqlConnection(Options.ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
        
        protected SqlConnection GetOpenConnection()
        {
            var connection = new SqlConnection(Options.ConnectionString);
            connection.Open();
            return connection;
        }

        protected static string GetClrTypeName(object item)
        {
            return item.GetType() + "," + item.GetType().Assembly.GetName().Name;
        }

        protected static string TableName(Type aggregateType)
        {
            return aggregateType.Name;
        }

        protected static string SnapshotTableName(Type aggregateType)
        {
            return $"{aggregateType.Name}_Snapshot";
        }
    }
}