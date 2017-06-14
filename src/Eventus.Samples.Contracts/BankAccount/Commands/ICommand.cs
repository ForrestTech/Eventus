using System;

namespace Eventus.Samples.Contracts.BankAccount.Commands
{
    public interface ICommand
    {
        Guid CorrelationId { get; }

        Guid AggregateId { get; }
    }
}