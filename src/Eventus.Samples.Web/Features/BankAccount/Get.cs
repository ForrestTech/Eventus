using System;
using System.Threading.Tasks;
using MediatR;
using Eventus.Samples.Core.ReadModel;

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
            private readonly IBankAccountReadModelRepository _repository;

            public Handler(IBankAccountReadModelRepository repository)
            {
                _repository = repository;
            }

            public async Task<BankAccountSummary> Handle(Query message)
            {
                var account = await _repository.GetAsync(message.Id)
                    .ConfigureAwait(false);

                return account;
            }
        }
    }
}