using System;

namespace Eventus.Samples.Contracts.BankAccount.Commands
{
    public class DepositFundsCommand : Command
    {
        public decimal Amount { get; set;  }

        public DepositFundsCommand(Guid correlationId, Guid accountId, decimal amount)
            : base(correlationId, accountId)
        {
            Amount = amount;
        }
    }
}