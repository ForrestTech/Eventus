using System;

namespace Eventus.Samples.Contracts.BankAccount.Commands
{
    public class CreateAccountCommand : Command
    {
        public string Name { get; set; }

        public CreateAccountCommand()
        {}

        public CreateAccountCommand(Guid correlationId, Guid accountId, string name)
            : base(correlationId, accountId)
        {
            Name = name;
        }
    }
}