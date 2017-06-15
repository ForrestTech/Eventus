using System;
using System.Threading.Tasks;
using Eventus.Samples.Web.Services;
using FluentValidation;
using MediatR;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class Withdrawal
    {
        public class Command : BaseCommand
        {
            public decimal Amount { get; set; }

            public Guid AccountId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.AccountId).NotEmpty();
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
                _client.Send("eventus.account.withdrawal", message);

                return Task.CompletedTask;
            }
        }
    }
}