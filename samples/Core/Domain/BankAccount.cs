namespace Eventus.Samples.Core.Domain
{
    using Eventus.Domain;
    using MassTransit;
    using Storage;
    using System;
    using System.Collections.Generic;

    public class BankAccount : Aggregate, ISnapshottable
    {
        public string Name { get; private set; }

        public decimal CurrentBalance { get; private set; }

        public List<Transaction> Transactions { get; private set; } = new();

        public BankAccount()
        {
            Transactions = new List<Transaction>();
        }

        public BankAccount(string name) : this(NewId.NextGuid(), name)
        {
        }

        public BankAccount(Guid id, string name) : base(id)
        {
            var accountCreated = new AccountCreatedEvent(Id, CurrentVersion, name);
            ApplyEvent(accountCreated);
        }

        public void Withdraw(decimal amount)
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

        #region ApplyEvents

        private void OnAccountCreated(AccountCreatedEvent @event)
        {
            Id = @event.AggregateId;
            Name = @event.Name;
            CurrentBalance = 0;
        }

        private void OnFundsDeposited(FundsDepositedEvent @event)
        {
            CurrentBalance += @event.Amount;
            var newTransaction = new Transaction(TransactionType.Deposit, @event.AggregateId, @event.Amount);
            Transactions.Add(newTransaction);
        }

        private void OnFundsWithdrawal(FundsWithdrawalEvent @event)
        {
            var newTransaction = new Transaction(TransactionType.Withdrawal, @event.AggregateId, @event.Amount);
            Transactions.Add(newTransaction);
            CurrentBalance -= @event.Amount;
        }

        #endregion

        #region Snapshot

        public Snapshot TakeSnapshot()
        {
            return new BankAccountSnapshot(NewId.NextGuid(),
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