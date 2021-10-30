namespace Eventus.SqlServer
{
    using Dapper;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    public class DatabaseConnectionLogger : IDatabaseConnectionLogger
    {
        private readonly ILogger<DatabaseConnectionLogger> _logger;

        public DatabaseConnectionLogger(ILogger<DatabaseConnectionLogger> logger)
        {
            _logger = logger;
        }
        
        public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection cnn, string sql, object? param = null)
        {
            _logger.LogDebug("Executing SQL Query: '{Sql}'", sql);
            
            return cnn.QueryAsync<T>(sql, param);
        }

        public Task<int> ExecuteAsync(IDbConnection connection, string sql, object? param = null)
        {
            _logger.LogDebug("Executing SQL: '{Sql}'", sql);
            
            return connection.ExecuteAsync(sql, param);
        }
    }
}