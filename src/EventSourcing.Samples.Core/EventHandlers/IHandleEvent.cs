using System.Threading.Tasks;
using EventSourcing.Events;

namespace EventSourcing.Samples.Core.EventHandlers
{
    public interface IHandleEvent<in TEvent> where TEvent : IEvent
    {
        Task Handle(TEvent @event);
    }
}