using System.Text.Json.Serialization;
using MassTransit;

namespace Eventus.Samples.Core.Domain
{
    using Events;
    using System;

    public class FundsDepositedEvent : Event
    {
        public decimal Amount { get; }

        public FundsDepositedEvent(Guid aggregateId, int targetVersion, decimal amount) : this(NewId.NextGuid(), aggregateId, targetVersion, amount)
        {
        }

        [JsonConstructor]
        public FundsDepositedEvent(Guid eventId, Guid aggregateId, int targetVersion, decimal amount) : base(aggregateId, targetVersion, eventId)
        {
            Amount = amount;
        }
    }
}
