namespace Eventus.Samples.Core.Domain
{
    using Events;
    using System;

    public class AccountCreatedEvent : Event
    {
        public string Name { get; protected set; }
        
        public AccountCreatedEvent(Guid aggregateId, int targetVersion, string name) : base(aggregateId, targetVersion)
        {
            Name = name;
        }
    }
}