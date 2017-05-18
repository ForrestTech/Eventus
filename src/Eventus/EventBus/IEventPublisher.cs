using System.Threading.Tasks;
using Eventus.Events;

namespace Eventus.EventBus
{
    public interface IEventPublisher
    {
        Task PublishAsync(IEvent @event);
    }
}