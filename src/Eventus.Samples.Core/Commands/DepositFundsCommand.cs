using System;
using Eventus.Commands;
using MediatR;

namespace Eventus.Samples.Core.Commands
{
    public class DepositFundsCommand : Command, IRequest
    {
        public decimal Amount { get; }

        public DepositFundsCommand(Guid correlationId, Guid accountId, decimal amount)
            : base(correlationId, accountId)
        {
            Amount = amount;
        }
    }
}