using System;
using EventSourcing.Repository;

namespace EventSourceDemo.Domain
{
    public class BankAccountSnapshot : Snapshot
    {
        public string Name { get; }

        public decimal CurrentBalance { get; }

        public BankAccountSnapshot(Guid id, Guid aggregateId, int version, string name, decimal currentBalance)
            : base(id, aggregateId, version)
        {
            Name = name;
            CurrentBalance = currentBalance;
        }
    }
}