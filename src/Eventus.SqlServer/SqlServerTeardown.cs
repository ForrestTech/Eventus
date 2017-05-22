using System.Threading.Tasks;
using Eventus.Cleanup;
using Respawn;

namespace Eventus.SqlServer
{
    public class SqlServerTeardown : SqlServerProviderBase, ITeardown
    {
        private static readonly Checkpoint Checkpoint = new Checkpoint();

        public SqlServerTeardown(string connectionString) : base(connectionString)
        {}

        public async Task TearDownAsync()
        {
            var connection = await GetOpenConnectionAsync().ConfigureAwait(false);
            using (connection)
            {
                Checkpoint.Reset(connection);
            }
        }
    }
}