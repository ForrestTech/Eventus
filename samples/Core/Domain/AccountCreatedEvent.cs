namespace Eventus.Samples.Core.Domain
{
    using Events;
    using System;

    public class AccountCreatedEvent : Event
    {
        public string Name { get; protected set; }

        public AccountCreatedEvent(Guid aggregateId, int version, Guid correlationId, string name) : base(aggregateId, version, correlationId)
        {
            Name = name;
        }
    }
}