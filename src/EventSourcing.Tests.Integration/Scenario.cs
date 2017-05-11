using System;
using System.Threading.Tasks;
using EventSourcing.Samples.Core.Commands;
using EventSourcing.Samples.Core.Domain;
using EventSourcing.Samples.Core.Handlers;
using EventSourcing.Samples.Infrastructure;
using EventSourcing.Samples.Infrastructure.Factories;
using FluentAssertions;
using Xunit;

namespace EventSourcing.Tests.Integration
{
    [Collection(StorageProvidersCollection.Name)]
    public class Scenario 
    {
        [Fact]
        public async Task RunBankingScenario()
        {
            var accountId = Guid.NewGuid();

            var repo = await RepositoryFactory.CreateAsync();
            var handler = new BankAccountCommandHandlers(repo);
            var joeBloggs = "Joe Bloggs";

            await handler.HandleAsync(new CreateAccountCommand(Guid.NewGuid(), accountId, joeBloggs));
            await handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 10));
            await handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 35));
            await handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 25));
            await handler.HandleAsync(new DepostiFundsCommand(Guid.NewGuid(), accountId, 5));
            await handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 10));

            var actual = await repo.GetByIdAsync<BankAccount>(accountId);

            actual.Id.Should().Be(accountId);
            actual.Name.Should().Be(joeBloggs);
            actual.CurrentBalance.Should().Be(15);
            actual.Transactions.Count.Should().Be(5);
        }
    }
}