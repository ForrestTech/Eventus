using System;
using System.Threading.Tasks;
using MediatR;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class Get
    {
        public class Query : IRequest<BankAccountSummary>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IAsyncRequestHandler<Query, BankAccountSummary>
        {
            public Task<BankAccountSummary> Handle(Query message)
            {
                return Task.FromResult(new BankAccountSummary
                {
                    Id = message.Id,
                    AccountName = "Joe",
                    CurrentBalance = 100
                });
            }
        }
    }
}