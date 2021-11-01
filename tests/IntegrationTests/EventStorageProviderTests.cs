namespace Eventus.IntegrationTests
{
    using Domain;
    using Factories;
    using FluentAssertions;
    using Samples.Core.Domain;
    using Storage;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class EventStorageProviderTests : IClassFixture<IntegrationTestsFixture>
    {
        private readonly ITestOutputHelper _output;

        public EventStorageProviderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [ClassData(typeof(ProviderTestsData))]
        public async Task GetLastEventAsync_should_return_last_event(string providerKey)
        {
            var provider = ProviderFactory.GetStorageProvider(providerKey, _output);

            var aggregate = new BankAccount("Account Name");

            await CommitChangesAsync(provider, aggregate);

            var actual = await provider.GetLastEventAsync(aggregate.GetType(), aggregate.Id);

            actual?.AggregateId.Should().Be(aggregate.Id);
            actual?.TargetVersion.Should().Be(0);
            actual.Should().BeOfType<AccountCreatedEvent>();
        }

        [Theory]
        [ClassData(typeof(ProviderTestsData))]
        public async Task GetEventsAsync_should_return_all_events_by_default(string providerKey)
        {
            var provider = ProviderFactory.GetStorageProvider(providerKey, _output);

            var aggregate = new BankAccount("Account Name");
            aggregate.Deposit(10);
            aggregate.Deposit(15);
            aggregate.Withdraw(5);

            await CommitChangesAsync(provider, aggregate);

            var actual = await provider.GetEventsAsync(aggregate.GetType(), aggregate.Id);

            actual.Count().Should().Be(4);
            actual.First().Should().BeOfType<AccountCreatedEvent>();
        }

        public static TheoryData<string, int, int, int> Data
        {
            get
            {
                var testSet = new TheoryData<int, int, int> {{0, 1, 1}, {0, 2, 2}, {0, 4, 4}, {0, 5, 4}};

                var data = new TheoryData<string, int, int, int>();

                //create a test for each provider
                ProviderFactory.Providers.ForEach(p =>
                {
                    testSet.ToList()
                        .ForEach(t =>
                        {
                            data.Add(p, (int)t.GetValue(0), (int)t.GetValue(1), (int)t.GetValue(2));
                        });
                });

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task GetEventsAsync_should_return_specified_range_of_events(string providerKey, int start,
            int count, int expected)
        {
            var provider = ProviderFactory.GetStorageProvider(providerKey, _output);

            var aggregate = new BankAccount("Account Name");
            aggregate.Deposit(10);
            aggregate.Deposit(15);
            aggregate.Withdraw(5);

            await CommitChangesAsync(provider, aggregate);

            var actual = await provider.GetEventsAsync(aggregate.GetType(), aggregate.Id, start, count);

            actual.Count().Should().Be(expected);
            actual.First().Should().BeOfType<AccountCreatedEvent>();
        }

        [Theory]
        [ClassData(typeof(ProviderTestsData))]
        public async Task Commit_should_support_committing_multiple_events(string providerKey)
        {
            var provider = ProviderFactory.GetStorageProvider(providerKey, _output);

            var aggregate = new BankAccount("Account Name");
            aggregate.Deposit(10);
            aggregate.Deposit(15);
            aggregate.Withdraw(5);

            await CommitChangesAsync(provider, aggregate);

            var actual = await provider.GetEventsAsync(aggregate.GetType(), aggregate.Id);

            actual.Count().Should().Be(4);
        }

        [Theory]
        [ClassData(typeof(ProviderTestsData))]
        public async Task Commit_target_version_order_should_be_preserved(string providerKey)
        {
            var provider = ProviderFactory.GetStorageProvider(providerKey, _output);

            var aggregate = new BankAccount("Account Name");
            aggregate.Deposit(10);
            aggregate.Deposit(15);
            aggregate.Withdraw(5);

            await CommitChangesAsync(provider, aggregate);

            var actual = await provider.GetEventsAsync(aggregate.GetType(), aggregate.Id);

            actual.Select(x => x.TargetVersion)
                .ToArray()
                .Should()
                .ContainInOrder(0, 1, 2, 3);
        }

        [Theory]
        [ClassData(typeof(ProviderTestsData))]
        public async Task Commit_should_support_committing_individual_events(string providerKey)
        {
            var provider = ProviderFactory.GetStorageProvider(providerKey, _output);

            var aggregate = new BankAccount("Account Name");

            await CommitChangesAsync(provider, aggregate);

            aggregate.Deposit(10);

            await CommitChangesAsync(provider, aggregate);

            aggregate.Deposit(15);

            await CommitChangesAsync(provider, aggregate);

            aggregate.Withdraw(5);

            await CommitChangesAsync(provider, aggregate);

            var actual = await provider.GetEventsAsync(aggregate.GetType(), aggregate.Id);

            actual.Count().Should().Be(4);
        }

        private static async Task CommitChangesAsync(IEventStorageProvider provider, Aggregate aggregate)
        {
            await provider.CommitChangesAsync(aggregate);
            
            aggregate.MarkChangesAsCommitted();
        }
    }
}
