using System;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.Core.Handlers;
using Eventus.Samples.Infrastructure.Factories;
using Serilog;

namespace Eventus.Samples.Console
{
    internal static class Program
    {
        private static void Main()
        {
            var log = new LoggerConfiguration()
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}")
                .MinimumLevel.Debug()
                .CreateLogger();

            Log.Logger = log;

            log.Information("Event sourcing sample");

            var accountId = Guid.NewGuid();

            var providerFactory = ProviderFactory.Current;
            var repo = providerFactory.CreateRepositoryAsync().Result;

            log.Information("Initialising provider");
            providerFactory.InitAsync().Wait();
            log.Information("Provider initialised");

            log.Information("Tearing down provider");
            var cleaner = providerFactory.CreateTeardownAsync().Result;
            cleaner.TearDownAsync().Wait();
            log.Information("StorageProviderFactory torn down");

            var handler = new BankAccountCommandHandlers(repo);

            log.Information("Running set of commands");

            handler.Handle(CreateAccountCommand.Create(Guid.NewGuid(), accountId, "Joe Bloggs")).Wait();
            handler.Handle(DepositFundsCommand.Create(Guid.NewGuid(), accountId, 10)).Wait();
            handler.Handle(DepositFundsCommand.Create(Guid.NewGuid(), accountId, 35)).Wait();
            handler.Handle(WithdrawFundsCommand.Create(Guid.NewGuid(), accountId, 25)).Wait();
            handler.Handle(DepositFundsCommand.Create(Guid.NewGuid(), accountId, 5)).Wait();
            handler.Handle(WithdrawFundsCommand.Create(Guid.NewGuid(), accountId, 10)).Wait();

            log.Information("Commands Run");

            log.Information("Get aggregate from store");

            var fromStore = repo.GetByIdAsync<BankAccount>(accountId).Result;

            log.Information($"Bank account ID: {fromStore.Id}");
            log.Information($"Balance: {fromStore.CurrentBalance}");
            log.Information($"Last committed version: {fromStore.LastCommittedVersion}");
            log.Information($"Transaction Count: {fromStore.Transactions.Count}");

            log.Information("Event sourcing sample ran");

            System.Console.ReadLine();
        }
    }
}
