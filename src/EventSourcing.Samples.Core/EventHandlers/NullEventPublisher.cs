using System.Threading.Tasks;
using EventSourcing.Event;
using EventSourcing.EventBus;

namespace EventSourcing.Samples.Core.EventHandlers
{
    public class NullEventPublisher : IEventPublisher
    {
        public Task PublishAsync(IEvent @event)
        {
            return Task.FromResult(0);
        }
    }
}
