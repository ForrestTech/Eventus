using System.Threading.Tasks;
using Eventus.Cleanup;

namespace Eventus.EventStore
{
    public class EventStoreTeardown : ITeardown
    {
        public Task TearDownAsync()
        {
            return Task.CompletedTask;
        }
    }
}