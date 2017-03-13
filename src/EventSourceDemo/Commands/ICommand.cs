using System;
using EventSourcing.Event;

namespace EventSourceDemo.Commands
{
    public interface ICommand : IMessage
    {
        Guid AggregateId { get; }
    }
}