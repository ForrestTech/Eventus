using System;
using Eventus.Events;

namespace Eventus.Samples.Core.Events
{
    public class FundsWithdrawalEvent : Event
    {
        public decimal Amount { get; protected set; }

        public FundsWithdrawalEvent(Guid aggregateId, int version, Guid correlationId, decimal amount) : base(aggregateId, version, correlationId)
        {
            Amount = amount;
        }
    }
}