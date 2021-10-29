namespace Eventus.Samples.Core.Domain
{
    using Events;
    using System;

    public class FundsDepositedEvent : Event
    {
        public decimal Amount { get; protected set; }

        public FundsDepositedEvent(Guid aggregateId, int version, Guid correlationId, decimal amount) : base(aggregateId, version, correlationId)
        {
            Amount = amount;
        }
    }
}