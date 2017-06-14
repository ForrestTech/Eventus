using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class Create
    {
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