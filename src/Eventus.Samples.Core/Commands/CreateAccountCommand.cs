using System;
using Eventus.Commands;
using MediatR;

namespace Eventus.Samples.Core.Commands
{
    public class CreateAccountCommand : Command, IRequest
    {
        public string Name { get; }

        public CreateAccountCommand(Guid correlationId, Guid accountId, string name)
            : base(correlationId, accountId)
        {
            Name = name;
        }
    }
}