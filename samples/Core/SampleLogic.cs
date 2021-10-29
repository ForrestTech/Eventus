namespace Eventus.Samples.Core
{
    using Domain;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Storage;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class SampleLogic
    {
        public static async Task Run(ServiceProvider serviceProvider)
        {
            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory?.CreateLogger("Sample");

            logger.LogInformation("Configured Eventus Sample");
            
            var repository = serviceProvider.GetService<IRepository>();
            Debug.Assert(repository != null, nameof(repository) + " != null");
            
            logger.LogInformation("Creating Account and processing transactions");
            
            var accountId = Guid.NewGuid();
            var account = new BankAccount(accountId, "Joe Blogs", Guid.NewGuid());
            account.Deposit(100, Guid.NewGuid());
            account.Withdraw(25, Guid.NewGuid());
            account.Withdraw(15, Guid.NewGuid());
            account.Deposit(10, Guid.NewGuid());

            logger.LogInformation("Saving account");

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
            
            logger.LogInformation("Eventus sample run complete");
        }
    }
}