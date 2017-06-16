using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eventus.Samples.Contracts;
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

            [DisplayName("Account Name")]
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
                _client.Send(Resources.BankAccountQueueName, new CreateAccountCommand
                {
                    CorrelationId = message.CorrelationId,
                    AggregateId = message.AccountId,
                    Name = message.AccountName
                });

                return Task.CompletedTask;
            }
        }
    }
}