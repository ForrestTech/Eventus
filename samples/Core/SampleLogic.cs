namespace Eventus.Samples.Core
{
    using Cleanup;
    using Domain;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Storage;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class SampleLogic
    {
        public static async Task Run(ServiceProvider serviceProvider)
        {
            var cleanup = serviceProvider.GetService<ITeardown>();
            if (cleanup != null)
            {
                await cleanup.TearDownAsync();
            }

            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory?.CreateLogger("Sample");

            logger.LogInformation("Configured Eventus Sample");
            
            var repository = serviceProvider.GetService<IRepository>();
            Debug.Assert(repository != null, nameof(repository) + " != null");
            
            logger.LogInformation("Creating Account and processing transactions");
            
            var account = new BankAccount("Joe Blogs");
            account.Deposit(100);
            account.Withdraw(25);
            account.Withdraw(15);
            account.Deposit(10);

            logger.LogInformation("Saving account");

            await repository.SaveAsync(account);

            logger.LogInformation("Getting account from storage");

            var fetched = await repository.GetByIdAsync<BankAccount>(account.Id);

            if (fetched != null)
            {
                logger.LogInformation("Account:{AccountId}", fetched.Id);
                logger.LogInformation("Current balance is: {CurrentBalance}", fetched.CurrentBalance);
                logger.LogInformation("Transactions:");

                fetched.Transactions.ForEach(x =>
                {
                    logger.LogInformation($"{x.Type}: {x.Amount}");
                });
                
                logger.LogInformation("Making extra changes to account");
            
                fetched.Withdraw(10);
                
                await repository.SaveAsync(fetched);
            }


            logger.LogInformation("Eventus sample run complete");
        }
    }
}