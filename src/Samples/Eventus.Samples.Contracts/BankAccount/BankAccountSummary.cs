using System;

namespace Eventus.Samples.Contracts.BankAccount
{
    public class BankAccountSummary
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }
    }
}