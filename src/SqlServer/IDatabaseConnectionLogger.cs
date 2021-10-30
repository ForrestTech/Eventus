namespace Eventus.SqlServer
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    public interface IDatabaseConnectionLogger
    {
        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection cnn, string sql, object? param = null);
        Task<int> ExecuteAsync(IDbConnection connection, string sql, object? param = null);
    }
}