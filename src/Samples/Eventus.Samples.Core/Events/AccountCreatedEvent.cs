using System;
using Eventus.Events;

namespace Eventus.Samples.Core.Events
{
    public class AccountCreatedEvent : Event
    {
        public string Name { get; protected set; }

        public AccountCreatedEvent(Guid aggregateId, int version, Guid correlationId, string name) : base(aggregateId, version, correlationId)
        {
            Name = name;
        }
    }
}