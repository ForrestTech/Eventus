namespace Eventus.IntegrationTests
{
    using Factories;
    using FluentAssertions;
    using Samples.Core.Domain;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class SnapshotStorageProviderTests
    {
        private readonly ITestOutputHelper _output;

        public SnapshotStorageProviderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [ClassData(typeof(ProviderTestsData))]
        public async Task SaveSnapshotAsync_should_store_current_aggregate_state(string providerKey)
        {
            var provider = ProviderFactory.GetSnapshotProvider(providerKey, _output);

            var aggregate = new BankAccount("Joe Blogs");

            var expected = aggregate.TakeSnapshot();

            await provider.SaveSnapshotAsync(aggregate.GetType(), expected);

            var actual = await provider.GetSnapshotAsync(aggregate.GetType(), aggregate.Id);

            actual?.Id.Should().Be(expected.Id);
            actual?.AggregateId.Should().Be(expected.AggregateId);
            actual?.Version.Should().Be(expected.Version);

            actual.As<BankAccountSnapshot>().Name.Should().Be(expected.As<BankAccountSnapshot>().Name);
            actual.As<BankAccountSnapshot>().CurrentBalance.Should().Be(expected.As<BankAccountSnapshot>().CurrentBalance);
            actual.As<BankAccountSnapshot>().Transactions.Should().BeEquivalentTo(expected.As<BankAccountSnapshot>().Transactions);
        }

        [Theory]
        [ClassData(typeof(ProviderTestsData))]
        public async Task GetSnapshotAsync_should_get_the_latest_snapshot(string providerKey)
        {
            var provider = ProviderFactory.GetSnapshotProvider(providerKey, _output);
            
            var aggregate = new BankAccount("Joe Blogs");

            var firstSnapshot = aggregate.TakeSnapshot();

            await provider.SaveSnapshotAsync(aggregate.GetType(), firstSnapshot);

            aggregate.Deposit(10);

            var expected = aggregate.TakeSnapshot();

            await provider.SaveSnapshotAsync(aggregate.GetType(), expected);

            var actual = await provider.GetSnapshotAsync(aggregate.GetType(), aggregate.Id);

            actual?.Id.Should().Be(expected.Id);
            actual?.AggregateId.Should().Be(expected.AggregateId);
            actual?.Version.Should().Be(expected.Version);

            actual.As<BankAccountSnapshot>().Name.Should().Be(expected.As<BankAccountSnapshot>().Name);
            actual.As<BankAccountSnapshot>().CurrentBalance.Should().Be(expected.As<BankAccountSnapshot>().CurrentBalance);
            actual.As<BankAccountSnapshot>().Transactions.Count.Should().Be(expected.As<BankAccountSnapshot>().Transactions.Count);
            actual.As<BankAccountSnapshot>().Transactions.Should().BeEquivalentTo(expected.As<BankAccountSnapshot>().Transactions);
        }
    }
}