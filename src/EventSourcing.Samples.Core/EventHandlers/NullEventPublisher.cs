using System.Threading.Tasks;
using EventSourcing.EventBus;
using EventSourcing.Events;

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
