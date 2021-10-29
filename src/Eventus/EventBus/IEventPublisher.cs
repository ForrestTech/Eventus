using System.Threading.Tasks;
using Eventus.Events;

namespace Eventus.EventBus
{
    /// <summary>
    /// Publishes events applied to an aggregate when they are stored
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync(IEvent @event);
    }
}