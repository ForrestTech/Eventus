using System;
using System.Threading.Tasks;
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

        public class Handler : IAsyncRequestHandler<BaseCommand>
        {
            public Handler()
            {

            }

            public Task Handle(BaseCommand message)
            {
                return Task.CompletedTask;
            }
        }
    }
}