using System;
using EventSourcing.Event;

namespace EventSourceDemo.Events
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