using System;
using Eventus.Commands;
using MediatR;

namespace Eventus.Samples.Core.Commands
{
    public class WithdrawFundsCommand : Command, IRequest
    {
        public decimal Amount { get; }

        public WithdrawFundsCommand(Guid correlationId, Guid accountId, decimal amount)
            : base(correlationId, accountId)
        {
            Amount = amount;
        }
    }
}