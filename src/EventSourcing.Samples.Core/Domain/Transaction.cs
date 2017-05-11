using System;

namespace EventSourcing.Samples.Core.Domain
{
    public class Transaction
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

        protected bool Equals(Transaction other)
        {
            return Type == other.Type && Id.Equals(other.Id) && Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Transaction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Amount.GetHashCode();
                return hashCode;
            }
        }
    }
}