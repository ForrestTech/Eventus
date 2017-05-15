using System;
using EventSourcing.Events;

namespace EventSourcing.Samples.Core.Commands
{
    public interface ICommand : IMessage
    {
        Guid AggregateId { get; }
    }
}