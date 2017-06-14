using System;
using System.Threading.Tasks;
using Eventus.Samples.Web.Services;
using FluentValidation;
using MediatR;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class Deposit
    {
        public class Command : BaseCommand
        {
            public decimal Amount { get; set; }

            public Guid AggregateId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.AggregateId).NotEmpty();
                RuleFor(x => x.CorrelationId).NotEmpty();
                RuleFor(x => x.Amount).GreaterThan(0);
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