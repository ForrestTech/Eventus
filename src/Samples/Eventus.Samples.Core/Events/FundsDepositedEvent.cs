using System;
using Eventus.Events;

namespace Eventus.Samples.Core.Events
{
    public class FundsDepositedEvent : Event
    {
        public decimal Amount { get; protected set; }

        public FundsDepositedEvent(Guid aggregateId, int version, Guid correlationId, decimal amount) : base(aggregateId, version, correlationId)
        {
            Amount = amount;
        }
    }
}