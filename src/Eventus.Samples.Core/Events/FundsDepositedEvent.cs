using System;
using Eventus.Events;

namespace Eventus.Samples.Core.Events
{
    public class FundsDepositedEvent : Event
    {
        public decimal Amount { get; protected set; }

        public FundsDepositedEvent(Guid aggregateId, int version, decimal amount) : base(aggregateId, version)
        {
            Amount = amount;
        }
    }
}