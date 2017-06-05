using System;
using Eventus.Commands;

namespace Eventus.Samples.Core.Commands
{
    public class DepositFundsCommand : Command
    {
        public decimal Amount { get; }

        public DepositFundsCommand(Guid correlationId, Guid accountId, decimal amount)
            : base(correlationId, accountId)
        {
            Amount = amount;
        }
    }
}