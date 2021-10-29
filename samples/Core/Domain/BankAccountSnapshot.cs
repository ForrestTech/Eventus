namespace Eventus.Samples.Core.Domain
{
    using Storage;
    using System;
    using System.Collections.Generic;

    public class BankAccountSnapshot : Snapshot
    {
        public string Name { get; }

        public decimal CurrentBalance { get; }

        public List<Transaction> Transactions { get; }

        public BankAccountSnapshot(Guid id, Guid aggregateId, int version, string name, decimal currentBalance,
            List<Transaction> transactions)
            : base(id, aggregateId, version)
        {
            Name = name;
            CurrentBalance = currentBalance;
            Transactions = transactions;
        }
    }
}