using System;

namespace Eventus.Samples.Core.ReadModel
{
    public class BankAccountSummary
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }
    }
}