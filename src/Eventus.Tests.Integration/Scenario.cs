﻿using System;
using System.Threading.Tasks;
using Eventus.Samples.Core.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.Core.Handlers;
using Eventus.Samples.Infrastructure.Factories;
using FluentAssertions;
using Xunit;

namespace Eventus.Tests.Integration
{
    [Collection(StorageProvidersCollection.Name)]
    public class Scenario 
    {
        [Fact]
        public async Task RunBankingScenario()
        {
            var accountId = Guid.NewGuid();

            var repo = await RepositoryFactory.CreateAsync()
                .ConfigureAwait(false);
            var handler = new BankAccountCommandHandlers(repo);
            const string joeBloggs = "Joe Bloggs";

            await handler.HandleAsync(new CreateAccountCommand(Guid.NewGuid(), accountId, joeBloggs)).ConfigureAwait(false);
            await handler.HandleAsync(new DepositFundsCommand(Guid.NewGuid(), accountId, 10)).ConfigureAwait(false);
            await handler.HandleAsync(new DepositFundsCommand(Guid.NewGuid(), accountId, 35)).ConfigureAwait(false);
            await handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 25)).ConfigureAwait(false);
            await handler.HandleAsync(new DepositFundsCommand(Guid.NewGuid(), accountId, 5)).ConfigureAwait(false);
            await handler.HandleAsync(new WithdrawFundsCommand(Guid.NewGuid(), accountId, 10)).ConfigureAwait(false);

            var actual = await repo.GetByIdAsync<BankAccount>(accountId).ConfigureAwait(false);

            actual.Id.Should().Be(accountId);
            actual.Name.Should().Be(joeBloggs);
            actual.CurrentBalance.Should().Be(15);
            actual.Transactions.Count.Should().Be(5);
        }
    }
}