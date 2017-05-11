using System.Threading.Tasks;
using EventSourcing.Cleanup;

namespace EventSourcing.EventStore
{
    public class EventStoreTeardown : ITeardown
    {
        public Task TearDownAsync()
        {
            return Task.CompletedTask;
        }
    }
}