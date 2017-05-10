using System.Threading.Tasks;
using EventSourcing.Event;

namespace EventSourceDemo.EventHandlers
{
    public interface IHandleEvent<in TEvent> where TEvent : IEvent
    {
        Task Handle(TEvent @event);
    }
}