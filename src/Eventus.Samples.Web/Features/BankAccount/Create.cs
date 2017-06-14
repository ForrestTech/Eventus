using System;
using System.Threading.Tasks;
using Eventus.Samples.Web.Services;
using FluentValidation;
using MediatR;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class Create
    {
        //use command specified in the web solution, this means we dont need to add MediatR to the contracts folder
        public class Command : BaseCommand
        {
            public Guid AggregateId { get; set; }

            public string AccountName { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.AccountName).NotEmpty();
                RuleFor(x => x.AggregateId).NotEmpty();
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
                _client.Send("eventus.account.create", message);

                return Task.CompletedTask;
            }
        }
    }
}