using System;

namespace Eventus.Events
{
    public interface IMessage
    {
        Guid CorrelationId { get; }
    }
}