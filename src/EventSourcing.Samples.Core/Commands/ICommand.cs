using System;
using EventSourcing.Event;

namespace EventSourcing.Samples.Core.Commands
{
    public interface ICommand : IMessage
    {
        Guid AggregateId { get; }
    }
}