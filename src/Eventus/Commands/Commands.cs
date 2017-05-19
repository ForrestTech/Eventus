using System;

namespace Eventus.Commands
{
    public class Command : ICommand
    {
        public Guid CorrelationId { get; }

        public Guid AggregateId { get; }

        public int TargetVersion { get; private set; }

        public Command(Guid correlationId, Guid aggregateId, int targetVersion = -1)
        {
            CorrelationId = correlationId;
            AggregateId = aggregateId;
            TargetVersion = targetVersion;
        }
    }
}
