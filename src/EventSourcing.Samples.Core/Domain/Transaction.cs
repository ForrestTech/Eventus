using System;

namespace EventSourceDemo.Domain
{
    public struct Transaction
    {
        public Transaction(TransactionType type, Guid id, decimal amount)
        {
            Type = type;
            Id = id;
            Amount = amount;
        }

        public TransactionType Type { get;  }

        public Guid Id { get; }

        public decimal Amount { get;  }
    }
}