namespace Eventus.Samples.Console
{
    using Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Samples.Core.Domain;
    using Storage;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using static System.Console;

    //TODO xml comment all the things
    static class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddEventus(options =>
            {
                options.SnapshotOptions.SnapshotFrequency = 5;
            });

            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory?.CreateLogger("Sample");

            logger.LogInformation("Configured Eventus Sample");

            logger.LogInformation("Creating Account and processing transactions");

            var accountId = Guid.NewGuid();
            var account = new BankAccount(accountId, "Joe Blogs", Guid.NewGuid());
            account.Deposit(100, Guid.NewGuid());
            account.Withdraw(25, Guid.NewGuid());
            account.Withdraw(15, Guid.NewGuid());
            account.Deposit(10, Guid.NewGuid());

            logger.LogInformation("Saving account");

            var repository = serviceProvider.GetService<IRepository>();
            Debug.Assert(repository != null, nameof(repository) + " != null");

            await repository.SaveAsync(account);

            logger.LogInformation("Getting account from storage");

            var fetched = await repository.GetByIdAsync<BankAccount>(accountId);

            if (fetched != null)
            {
                logger.LogInformation("Account:{AccountId}", accountId);
                logger.LogInformation("Current balance is: {CurrentBalance}", fetched.CurrentBalance);
                logger.LogInformation("Transactions:");

                fetched.Transactions.ForEach(x =>
                {
                    logger.LogInformation($"{x.Type}: {x.Amount}");
                });
            }

            ReadLine();
        }
    }
}