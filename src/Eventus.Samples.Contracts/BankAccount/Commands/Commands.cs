using System;

namespace Eventus.Samples.Contracts.BankAccount.Commands
{
    public class Command : ICommand
    {
        public Guid CorrelationId { get; set; }

        public Guid AggregateId { get; set; }

        public int TargetVersion { get; set; }

        public Command(Guid correlationId, Guid aggregateId) : this(correlationId, aggregateId, -1)
        { }

        public Command(Guid correlationId, Guid aggregateId, int targetVersion)
        {
            if (correlationId == Guid.Empty) throw new ArgumentException(nameof(correlationId));
            if (aggregateId == Guid.Empty) throw new ArgumentException(nameof(aggregateId));

            CorrelationId = correlationId;
            AggregateId = aggregateId;
            TargetVersion = targetVersion;
        }
    }
}
