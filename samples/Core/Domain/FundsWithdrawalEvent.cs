namespace Eventus.Samples.Core.Domain
{
    using Events;
    using System;

    public class FundsWithdrawalEvent : Event
    {
        public decimal Amount { get; protected set; }

        public FundsWithdrawalEvent(Guid aggregateId, int targetVersion, Guid correlationId, decimal amount) : base(aggregateId, targetVersion, correlationId)
        {
            Amount = amount;
        }
    }
}