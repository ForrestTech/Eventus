namespace Eventus.Samples.Console
{
    using Extensions.DependencyInjection;
    using InMemory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Samples.Core.Domain;
    using Storage;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using static System.Console;

    //TODO create separate in memory package
    //TODO create event configuration for setting like snapshot frequency
    //TODO create separate console app sample for in memory
    //TODO wire up logging decorators (most logging is debug level)
    //TODO xml comment all the things
    static class Program
    {
        static async Task Main(string[] args)
        {
            WriteLine("Configuring Eventus Sample");
            
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
            services.AddEventus(config =>
            {
                config.UseInMemoryStorage();
            });
            
            var serviceProvider = services.BuildServiceProvider();            
            
            var repository = serviceProvider.GetService<IRepository>();
            Debug.Assert(repository != null, nameof(repository) + " != null");

            WriteLine("Creating Account and processing transactions");
            
            var accountId = Guid.NewGuid();
            var account = new BankAccount(accountId, "Joe Blogs", Guid.NewGuid());
            account.Deposit(100, Guid.NewGuid());
            account.Withdraw(25, Guid.NewGuid());
            account.Withdraw(15, Guid.NewGuid());
            account.Deposit(10, Guid.NewGuid());

            WriteLine("Saving account");

            await repository.SaveAsync(account);

            WriteLine("Getting account from storage");
            
            var fetched = await repository.GetByIdAsync<BankAccount>(accountId);

            if (fetched != null)
            {
                WriteLine($"Account:{accountId}");
                WriteLine($"Current balance is: {fetched.CurrentBalance}");
                WriteLine("Transactions:");
                
                fetched.Transactions.ForEach(x =>
                {
                    WriteLine($"{x.Type}: {x.Amount}");
                });
            }

            ReadLine();
        }
    }
}