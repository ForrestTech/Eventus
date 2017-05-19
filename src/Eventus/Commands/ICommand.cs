using System;
using Eventus.Events;

namespace Eventus.Commands
{
    public interface ICommand : IMessage
    {
        Guid AggregateId { get; }
    }
}