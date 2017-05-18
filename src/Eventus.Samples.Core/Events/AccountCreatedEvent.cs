using System;
using Eventus.Events;

namespace Eventus.Samples.Core.Events
{
    public class AccountCreatedEvent : Event
    {
        public string Name { get; protected set; }

        public AccountCreatedEvent(Guid aggregateId, int version, string name) : base(aggregateId, version)
        {
            Name = name;
        }
    }
}