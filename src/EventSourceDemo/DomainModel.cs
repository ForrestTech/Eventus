using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using EventSourceDemo;

namespace EventSourceDemo
{

    public abstract class Aggregate
    {
        internal readonly List<Event> Events = new List<Event>();

        public Guid Id { get; protected set; }

        public int Version { get; internal set; } = -1;

        public IEnumerable<Event> GetUncommittedEvents()
        {
            return Events;
        }

        public void MarkEventsAsCommitted()
        {
            Events.Clear();
        }

        public void LoadStateFromHistory(IEnumerable<Event> history)
        {
            foreach (var e in history) ApplyEvent(e, false);
        }

        protected internal void ApplyEvent(Event @event)
        {
            ApplyEvent(@event, true);
        }

        protected virtual void ApplyEvent(Event @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);
            if (isNew)
            {
                @event.Version = ++Version;
                Events.Add(@event);
            }
            else
            {
                Version = @event.Version;
            }
        }
    }

    public abstract class ReadModelAggregate : Aggregate
    {
        protected internal void ApplyEvent(Event @event, int version)
        {
            @event.Version = version;
            ApplyEvent(@event, true);
        }

        protected override void ApplyEvent(Event @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);
            if (isNew)
            {
                Events.Add(@event);
            }
            Version++;

        }
    }


    public class Wallet : Aggregate
    {
        public string Name { get; private set; }

        public decimal CurrentBalance { get; private set; }

        public List<Transaction> Transactions { get; }

        public Wallet()
        {
            Transactions = new List<Transaction>();
        }
        
        private void Apply(AccountCreatedEvent @event)
        {
            Id = @event.Id;
            Name = @event.Name;
            CurrentBalance = 0;
        }

        private void Apply(FundsDepositedEvent @event)
        {
            var newTransaction = new Transaction { Id = @event.Id, Amount = @event.Amount };
            Transactions.Add(newTransaction);
            CurrentBalance = CurrentBalance + @event.Amount;
        }

        private void Apply(FundsWithdrawalEvent @event)
        {
            var newTransaction = new Transaction { Id = @event.Id, Amount = @event.Amount };
            Transactions.Add(newTransaction);
            CurrentBalance = CurrentBalance - @event.Amount;
        }
    }

    public struct Transaction
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }
    }


}