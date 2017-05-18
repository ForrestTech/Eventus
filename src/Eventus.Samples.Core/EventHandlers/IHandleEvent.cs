using System.Threading.Tasks;
using Eventus.Events;

namespace Eventus.Samples.Core.EventHandlers
{
    public interface IHandleEvent<in TEvent> where TEvent : IEvent
    {
        Task Handle(TEvent @event);
    }
}