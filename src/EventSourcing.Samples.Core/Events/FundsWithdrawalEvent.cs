using System;

namespace EventSourcing.Samples.Core.Events
{
    public class FundsWithdrawalEvent : Event.Event
    {
        public decimal Amount { get; protected set; }

        public FundsWithdrawalEvent(Guid aggregateId, int version, decimal amount) : base(aggregateId, version)
        {
            Amount = amount;
        }
    }
}