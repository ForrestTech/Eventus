using System;
using Eventus.Samples.Web.Features.BankAccount.DTO;
using MediatR;

namespace Eventus.Samples.Web.Features.BankAccount.Query
{
    public class GetBankAccountQuery : IRequest<BankAccountSummary>
    {
        public Guid Id { get; set; }
    }
}