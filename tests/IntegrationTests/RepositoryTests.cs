using Xunit.Abstractions;

namespace Eventus.IntegrationTests
{
    using Factories;
    using FluentAssertions;
    using Samples.Core.Domain;
    using System.Threading.Tasks;
    using Xunit;

    public class RepositoryTests : IClassFixture<IntegrationTestsFixture>
    {
        private readonly ITestOutputHelper _output;

        public RepositoryTests(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Theory]
        [ClassData(typeof(ProviderTestsData))]
        public async Task RunBankingScenario(string providerKey)
        {
            var repo = ProviderFactory.CreateRepository(providerKey, _output);

            string joeBloggs = "Joe Bloggs";

            var account = new BankAccount(joeBloggs);

            account.Deposit(10);
            account.Deposit(35);
            account.Withdraw(25);
            account.Deposit(5);
            account.Withdraw(10);

            var actual = await repo.GetByIdAsync<BankAccount>(account.Id);

            actual?.Id.Should().Be(account.Id);
            actual?.Name.Should().Be(joeBloggs);
            actual?.CurrentBalance.Should().Be(15);
            actual?.Transactions.Count.Should().Be(5);
        }
    }
}
