using System;
using System.Collections.Generic;
using Eventus.Domain;
using Eventus.Samples.Core.Events;
using Eventus.Storage;

namespace Eventus.Samples.Core.Domain
{
    public class BankAccount : Aggregate, ISnapshottable
    {
        public string Name { get; private set; }

        public decimal CurrentBalance { get; private set; }

        public List<Transaction> Transactions { get; private set; }

        public BankAccount()
        {
            //Important: AggregateType roots must have a parameterless constructor
            //to make it easier to construct from scratch.

            Transactions = new List<Transaction>();
        }

        public BankAccount(Guid id, string name, Guid correlationId = new Guid()) : this()
        {
            //Pattern: Create the event and call ApplyEvent(Event)
            var accountCreated = new AccountCreatedEvent(id, CurrentVersion, correlationId, name);
            ApplyEvent(accountCreated);
        }

        public void WithDrawFunds(decimal amount, Guid correlationId = new Guid())
        {
            if (CurrentBalance >= amount)
            {
                var withdraw = new FundsWithdrawalEvent(Id, CurrentVersion, correlationId, amount);
                ApplyEvent(withdraw);
            }
        }

        public void Deposit(decimal amount, Guid correlationId = new Guid())
        {
            var deposit = new FundsDepositedEvent(Id, CurrentVersion, correlationId, amount);
            ApplyEvent(deposit);
        }

        #region ApplyEvents

        private void OnAccountCreated(AccountCreatedEvent @event)
        {
            Id = @event.AggregateId;
            Name = @event.Name;
            CurrentBalance = 0;
        }

        private void OnFundsDeposited(FundsDepositedEvent @event)
        {
            CurrentBalance = CurrentBalance + @event.Amount;
            var newTransaction = new Transaction(TransactionType.Deposit, @event.AggregateId, @event.Amount);
            Transactions.Add(newTransaction);
        }

        private void OnFundsWithdrawl(FundsWithdrawalEvent @event)
        {
            var newTransaction = new Transaction(TransactionType.Withdrawal, @event.AggregateId, @event.Amount);
            Transactions.Add(newTransaction);
            CurrentBalance = CurrentBalance - @event.Amount;
        }

        #endregion

        #region Snapshot

        public Snapshot TakeSnapshot()
        {
            return new BankAccountSnapshot(Guid.NewGuid(),
                Id,
                CurrentVersion,
                Name,
                CurrentBalance,
                Transactions);
        }

        public void ApplySnapshot(Snapshot snapshot)
        {
            var item = (BankAccountSnapshot)snapshot;

            Id = item.AggregateId;
            CurrentVersion = item.Version;
            LastCommittedVersion = item.Version;
            Name = item.Name;
            CurrentBalance = item.CurrentBalance;
            Transactions = item.Transactions;
        }

        #endregion
    }
}