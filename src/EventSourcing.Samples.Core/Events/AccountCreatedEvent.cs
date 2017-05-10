using System;

namespace EventSourcing.Samples.Core.Events
{
    public class AccountCreatedEvent : Event.Event
    {
        public string Name { get; protected set; }

        public AccountCreatedEvent(Guid aggregateId, int version, string name) : base(aggregateId, version)
        {
            Name = name;
        }
    }
}