using System;

namespace EventSourcing.Event
{
    public interface IMessage
    {
        Guid CorrelationId { get; }
    }
}