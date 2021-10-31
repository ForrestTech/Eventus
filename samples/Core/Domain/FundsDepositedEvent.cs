namespace Eventus.Samples.Core.Domain
{
    using Events;
    using System;

    public class FundsDepositedEvent : Event
    {
        public decimal Amount { get; }

        public FundsDepositedEvent(Guid aggregateId, int targetVersion, decimal amount) : base(aggregateId, targetVersion)
        {
            Amount = amount;
        }
    }
}