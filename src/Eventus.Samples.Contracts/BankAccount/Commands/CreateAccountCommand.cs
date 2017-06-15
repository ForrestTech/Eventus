using System;

namespace Eventus.Samples.Contracts.BankAccount.Commands
{
    public class CreateAccountCommand : Command
    {
        public string Name { get; set; }

        public static CreateAccountCommand Create(Guid correlationToken, Guid aggregateId, string name)
        {
            return new CreateAccountCommand {CorrelationId = correlationToken, AggregateId = aggregateId, Name = name};
        }
    }
}