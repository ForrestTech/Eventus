using System;

namespace Eventus.Samples.Contracts.BankAccount.Commands
{
    public class WithdrawFundsCommand : Command
    {
        public decimal Amount { get; set; }

        public static WithdrawFundsCommand Create(Guid correlationToken, Guid aggregateId, decimal amount)
        {
            return new WithdrawFundsCommand { CorrelationId = correlationToken, AggregateId = aggregateId, Amount = amount };
        }
    }
}