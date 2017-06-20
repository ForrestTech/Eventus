using System;

namespace Eventus.Samples.Contracts.BankAccount.Commands
{
    public class Command : ICommand
    {
        public Guid CorrelationId { get; set; }

        public Guid AggregateId { get; set; }

        public int TargetVersion { get; set; }
    }
}
