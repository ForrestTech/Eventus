using System;
using System.Threading.Tasks;
using Eventus.Samples.Web.Features.BankAccount.DTO;
using Eventus.Samples.Web.Features.BankAccount.Query;
using MediatR;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class GetBankAccountHandler : IAsyncRequestHandler<GetBankAccountQuery, BankAccountSummary>
    {
        public Task<BankAccountSummary> Handle(GetBankAccountQuery message)
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