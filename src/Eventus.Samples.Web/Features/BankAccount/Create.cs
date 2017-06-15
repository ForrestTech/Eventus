using System;
using System.Threading.Tasks;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Web.Services;
using FluentValidation;
using MediatR;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class Create
    {
        //Using command specified in the web solution, this means we don't need to add MediatR to the contracts project
        public class Command : BaseCommand
        {
            public Guid AccountId { get; set; }

            public string AccountName { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.AccountName).NotEmpty();
                RuleFor(x => x.AccountId).NotEmpty();
                RuleFor(x => x.CorrelationId).NotEmpty();
            }
        }

        public class Handler : IAsyncRequestHandler<Command>
        {
            private readonly RabbitMQClient _client;

            public Handler(RabbitMQClient client)
            {
                _client = client;
            }

            public Task Handle(Command message)
            {
                //todo move queue name to constants
                _client.Send("eventus.account.create", new CreateAccountCommand(message.CorrelationId, message.AccountId, message.AccountName));

                return Task.CompletedTask;
            }
        }
    }
}