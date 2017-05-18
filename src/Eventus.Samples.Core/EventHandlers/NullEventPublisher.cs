using System.Threading.Tasks;
using Eventus.EventBus;
using Eventus.Events;

namespace Eventus.Samples.Core.EventHandlers
{
    public class NullEventPublisher : IEventPublisher
    {
        public Task PublishAsync(IEvent @event)
        {
            return Task.FromResult(0);
        }
    }
}
