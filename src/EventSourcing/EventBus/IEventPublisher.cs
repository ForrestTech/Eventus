using System.Threading.Tasks;
using EventSourcing.Events;

namespace EventSourcing.EventBus
{
    public interface IEventPublisher
    {
        Task PublishAsync(IEvent @event);
    }
}