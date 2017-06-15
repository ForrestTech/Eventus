using System;

namespace Eventus.Samples.Contracts.BankAccount.Commands
{
    public class DepositFundsCommand : Command
    {
        public decimal Amount { get; set;  }

        public static DepositFundsCommand Create(Guid correlationToken, Guid aggregateId, decimal amount)
        {
            return new DepositFundsCommand { CorrelationId = correlationToken, AggregateId = aggregateId, Amount = amount };
        }
    }
}