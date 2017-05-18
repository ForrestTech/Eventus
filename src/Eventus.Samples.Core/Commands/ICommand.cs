using System;
using Eventus.Events;

namespace Eventus.Samples.Core.Commands
{
    public interface ICommand : IMessage
    {
        Guid AggregateId { get; }
    }
}