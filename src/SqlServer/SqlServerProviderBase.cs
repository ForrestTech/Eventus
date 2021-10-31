namespace Eventus.SqlServer
{
    using Configuration;
    using Microsoft.Data.SqlClient;
    using System;
    using System.Threading.Tasks;

    public abstract class SqlServerProviderBase
    {
        private readonly EventusSqlServerOptions _sqlOptions;
        protected readonly EventusOptions Options;

        protected SqlServerProviderBase(EventusSqlServerOptions sqlOptions, EventusOptions options)
        {
            _sqlOptions = sqlOptions;
            Options = options;
        }

        protected async Task<SqlConnection> GetOpenConnectionAsync()
        {
            var connection = new SqlConnection(_sqlOptions.ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
        
        protected SqlConnection GetOpenConnection()
        {
            var connection = new SqlConnection(_sqlOptions.ConnectionString);
            connection.Open();
            return connection;
        }

        protected static string GetClrTypeName(object item)
        {
            return item.GetType() + "," + item.GetType().Assembly.GetName().Name;
        }

        protected static string TableName(Type aggregateType)
        {
            return SqlSchemaHelper.TableName(aggregateType);
        }

        protected static string SnapshotTableName(Type aggregateType)
        {
            return SqlSchemaHelper.SnapshotTableName(aggregateType);
        }
    }
}