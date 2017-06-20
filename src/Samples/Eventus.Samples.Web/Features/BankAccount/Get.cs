using System;
using System.Threading.Tasks;
using MediatR;
using Eventus.Samples.Contracts.BankAccount;
using Eventus.Samples.ReadLayer;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class Get
    {
        public class Query : IRequest<BankAccountSummary>
        {
            public Guid AccountId { get; set; }
        }

        public class Handler : IAsyncRequestHandler<Query, BankAccountSummary>
        {
            private readonly BankAccountReadRepository _repository;

            public Handler(BankAccountReadRepository repository)
            {
                _repository = repository;
            }

            public Task<BankAccountSummary> Handle(Query message)
            {
                var account = _repository.Get(message.AccountId);

                return Task.FromResult(account);
            }
        }
    }
}