using System;

namespace EventSourcing.Events
{
    public interface IMessage
    {
        Guid CorrelationId { get; }
    }
}