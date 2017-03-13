using System;
using Newtonsoft.Json;

namespace EventSourceDemo
{
    public interface IMessage
    {
    }

    public interface IEvent : IMessage
    {
        Guid Id { get; }
        int Version { get; }
    }

    public class Event : IEvent
    {
        public Guid Id { get; protected set; }
        public int Version { get; protected internal set; }
    }

    public class AccountCreatedEvent : Event
    {
        public string Name { get; protected set; }

        public AccountCreatedEvent(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        [JsonConstructor]
        private AccountCreatedEvent(Guid id, string name, int version) : this(id, name)
        {
            Version = version;
        }
    }

    public class FundsDepositedEvent : Event
    {
        public decimal Amount { get; protected set; }
        
        public FundsDepositedEvent(Guid id, decimal amount)
        {
            Id = id;
            Amount = amount;
        }

        [JsonConstructor]
        private FundsDepositedEvent(Guid id, decimal amount, int version) : this(id, amount)
        {
            Version = version;
        }
    }

    public class FundsWithdrawalEvent : Event
    {
        public decimal Amount { get; protected set; }
        
        public FundsWithdrawalEvent(Guid id, decimal amount)
        {
            Id = id;
            Amount = amount;
        }

        [JsonConstructor]
        private FundsWithdrawalEvent(Guid id, decimal amount, int version) : this(id, amount)
        {
            Version = version;
        }
    }
}