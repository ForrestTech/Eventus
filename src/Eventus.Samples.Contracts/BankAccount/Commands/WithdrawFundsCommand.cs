using System;

namespace Eventus.Samples.Contracts.BankAccount.Commands
{
    public class WithdrawFundsCommand : Command
    {
        public decimal Amount { get; set; }

        public WithdrawFundsCommand(Guid correlationId, Guid accountId, decimal amount)
            : base(correlationId, accountId)
        {
            Amount = amount;
        }
    }
}