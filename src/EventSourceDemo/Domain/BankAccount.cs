using System;
using System.Collections.Generic;
using EventSourceDemo.Events;
using EventSourcing.Domain;
using EventSourcing.Repository;

namespace EventSourceDemo.Domain
{
    public class BankAccount : Aggregate, ISnapshottable
    {
        public string Name { get; private set; }

        public decimal CurrentBalance { get; private set; }

        public List<Transaction> Transactions { get; }

        public BankAccount()
        {
            //Important: Aggregate roots must have a parameterless constructor
            //to make it easier to construct from scratch.

            Transactions = new List<Transaction>();
        }

        public BankAccount(Guid id, string name)
        {
            //Pattern: Create the event and call ApplyEvent(Event)
            var accountCreated = new AccountCreatedEvent(id, CurrentVersion, name);
            ApplyEvent(accountCreated);
        }

        public void WithDrawFunds(decimal amount)
        {
            if (CurrentBalance >= amount)
            {
                var withdraw = new FundsWithdrawalEvent(Id, CurrentVersion, amount);
                ApplyEvent(withdraw);
            }
        }

        public void Deposit(decimal amount)
        {
            var deposit = new FundsDepositedEvent(Id, CurrentVersion, amount);
            ApplyEvent(deposit);
        }

        private void Apply(AccountCreatedEvent @event)
        {
            Id = @event.AggregateId;
            Name = @event.Name;
            CurrentBalance = 0;
        }

        private void Apply(FundsDepositedEvent @event)
        {
            var newTransaction = new Transaction(TransactionType.Deposit, @event.AggregateId, @event.Amount);
            Transactions.Add(newTransaction);
            CurrentBalance = CurrentBalance + @event.Amount;
        }

        private void Apply(FundsWithdrawalEvent @event)
        {
            var newTransaction = new Transaction(TransactionType.Withdrawal, @event.AggregateId, @event.Amount);
            Transactions.Add(newTransaction);
            CurrentBalance = CurrentBalance - @event.Amount;
        }

        public Snapshot TakeSnapshot()
        {
            return new BankAccountSnapshot(Guid.NewGuid(),
                Id,
                CurrentVersion,
                Name,
                CurrentBalance);
        }

        public void ApplySnapshot(Snapshot snapshot)
        {
            var item = (BankAccountSnapshot)snapshot;

            Id = item.AggregateId;
            CurrentVersion = item.Version;
            LastCommittedVersion = item.Version;
            Name = item.Name;
            CurrentBalance = item.CurrentBalance;
        }
    }
}