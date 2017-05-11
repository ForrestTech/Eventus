using System;
using EventSourcing.Samples.Core.Commands;
using EventSourcing.Samples.Core.Domain;
using EventSourcing.Samples.Core.Handlers;
using EventSourcing.Samples.Infrastructure.Factories;
using static System.Console;

namespace EventSourcing.Samples.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLine("Event sourcing sample");

            WriteLine("Tearing down provider");

            var cleaner = TearDownFactory.Create();
            cleaner.TearDownAsync().Wait();

            WriteLine("Provider torn down");

            var accountId = Guid.NewGuid();

            var repo = RepositoryFactory.CreateAsync().Result;

            var handler = new BankAccountCommandHandlers(repo);

            WriteLine("Running set of commands");

            handler.HandleAsync(new CreateAccountCommand(Guid.NewGuid(), accountId, "Joe Bloggs")).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 35)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 25)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 5)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();

            WriteLine("Commands Run");

            WriteLine("Get aggregate from store");

            var fromStore = repo.GetByIdAsync<BankAccount>(accountId).Result;

            WriteLine($"Bank account ID: {fromStore.Id}");
            WriteLine($"Balance: {fromStore.CurrentBalance}");
            WriteLine($"Last commited version: {fromStore.LastCommittedVersion}");
            WriteLine($"Transaction Count: {fromStore.Transactions.Count}");

            WriteLine("Event sourcing sample ran");

            ReadLine();
        }
    }
}
