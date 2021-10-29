namespace Eventus.Samples.Core.Domain
{
    using Events;
    using System;

    public class FundsDepositedEvent : Event
    {
        public decimal Amount { get; protected set; }

        public FundsDepositedEvent(Guid aggregateId, int targetVersion, Guid correlationId, decimal amount) : base(aggregateId, targetVersion, correlationId)
        {
            Amount = amount;
        }
    }
}