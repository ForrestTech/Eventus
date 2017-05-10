using System;

namespace EventSourceDemo.ReadModel
{
    public class AccountSummary
    {
        public Guid Id { get; set; }

        public decimal Balance { get; set; }
    }
}