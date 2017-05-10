using System.Threading.Tasks;
using EventSourcing.Event;

namespace EventSourcing.Samples.Core.EventHandlers
{
    public interface IHandleEvent<in TEvent> where TEvent : IEvent
    {
        Task Handle(TEvent @event);
    }
}