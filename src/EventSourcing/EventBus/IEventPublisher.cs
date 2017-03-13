using System.Threading.Tasks;
using EventSourcing.Event;

namespace EventSourcing.EventBus
{
    public interface IEventPublisher
    {
        Task PublishAsync(IEvent @event);
    }
}