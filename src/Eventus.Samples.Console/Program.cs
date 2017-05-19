using System;
using Eventus.Samples.Core.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.Core.Handlers;
using Eventus.Samples.Infrastructure.Factories;
using Serilog;

namespace Eventus.Samples.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}")
                .MinimumLevel.Debug()
                .CreateLogger();

            Log.Logger = log;

            log.Information("Event sourcing sample");

            log.Information("Tearing down provider");

            var cleaner = TearDownFactory.Create();
            cleaner.TearDownAsync().Wait();

            log.Information("StorageProvider torn down");

            var accountId = Guid.NewGuid();

            var repo = RepositoryFactory.CreateAsync(true, true).Result;

            var handler = new BankAccountCommandHandlers(repo);

            log.Information("Running set of commands");

            handler.HandleAsync(new CreateAccountCommand(Guid.NewGuid(), accountId, "Joe Bloggs")).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 35)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 25)).Wait();
            handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 5)).Wait();
            handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 10)).Wait();

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
