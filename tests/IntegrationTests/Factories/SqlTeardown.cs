namespace Eventus.IntegrationTests.Factories
{
    using Microsoft.Data.SqlClient;
    using Respawn;
    using System.Threading.Tasks;

    public class SqlServerTeardown:  ITeardown
    {
        private readonly string _connectionString;
        private static readonly Checkpoint Checkpoint = new();

        public SqlServerTeardown(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task TearDownAsync()
        {
            var connection = await GetOpenConnectionAsync();
            await using (connection)
            {
                await Checkpoint.Reset(connection);
            }
        }

        private async Task<SqlConnection> GetOpenConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}