using System.Threading.Tasks;
using Eventus.Cleanup;

namespace Eventus.SqlServer
{
    public class SqlServerTeardown : ITeardown
    {
        public Task TearDownAsync()
        {
            return Task.CompletedTask;
        }
    }
}