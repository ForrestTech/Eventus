using System;
using Eventus.Commands;

namespace Eventus.Samples.Core.Commands
{
    public class WithdrawFundsCommand : Command
    {
        public decimal Amount { get; }

        public WithdrawFundsCommand(Guid correlationId, Guid accountId, decimal amount)
            : base(correlationId, accountId)
        {
            Amount = amount;
        }
    }
}