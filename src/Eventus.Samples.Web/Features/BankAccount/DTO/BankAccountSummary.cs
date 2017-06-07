using System;

namespace Eventus.Samples.Web.Features.BankAccount.DTO
{
    public class BankAccountSummary
    {
        public Guid Id { get; set; }

        public string AccountName { get; set; }

        public int CurrentBalance { get; set; }
    }
}