using System;
using System.Linq;
using System.Threading.Tasks;
using EventSourceDemo.Domain;
using EventSourceDemo.Events;
using EventSourcing.DocumentDb;
using EventSourcing.Domain;
using EventSourcing.Samples.Infrastructure;
using FluentAssertions;
using Xunit;

namespace EventSourcing.Tests.Integration
{
    [Collection(DocumetDbCollection.Name)]
    public class DocumentDbStorageProviderTest
    {
        private readonly DocumentDbStorageProvider _provider;

        public DocumentDbStorageProviderTest()
        {
            _provider = DocumentDbFactory.CreateDocumentDbStorageProvider();
            _provider.InitAsync(DocumentDbFactory.DocumentDbConfig).Wait();
        }

        [Fact]
        public async Task GetLastEventAsync_should_return_last_event()
        {
            var aggregateId = Guid.NewGuid();

            var aggregate = new BankAccount(aggregateId, "Account Name");

            await CommitChanges(aggregate);

            var actual = await _provider.GetLastEventAsync(aggregate.GetType(), aggregateId);

            //todo support time mocking
            actual.AggregateId.Should().Be(aggregateId);
            actual.TargetVersion.Should().Be(-1);
            actual.Should().BeOfType<AccountCreatedEvent>();
        }

        [Fact]
        public async Task GetEventsAsync_should_return_all_events_by_default()
        {
            var aggregateId = Guid.NewGuid();

            var aggregate = new BankAccount(aggregateId, "Account Name");
            aggregate.Deposit(10);
            aggregate.Deposit(15);
            aggregate.WithDrawFunds(5);

            await CommitChanges(aggregate);

            var actual = await _provider.GetEventsAsync(aggregate.GetType(), aggregateId);

            actual.Count().Should().Be(4);
            actual.First().Should().BeOfType<AccountCreatedEvent>();
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(0, 1, 2)]
        [InlineData(0, 4, 4)]
        [InlineData(0, 5, 4)]
        public async Task GetEventsAsync_should_return_specified_range_of_events(int start, int count, int expected)
        {
            var aggregateId = Guid.NewGuid();

            var aggregate = new BankAccount(aggregateId, "Account Name");
            aggregate.Deposit(10);
            aggregate.Deposit(15);
            aggregate.WithDrawFunds(5);

            await CommitChanges(aggregate);

            var actual = await _provider.GetEventsAsync(aggregate.GetType(), aggregateId, start, count);

            actual.Count().Should().Be(expected);
            actual.First().Should().BeOfType<AccountCreatedEvent>();
        }
        
        [Fact]
        public async Task Commit_should_support_committing_multiple_events()
        {
            var aggregateId = Guid.NewGuid();

            var aggregate = new BankAccount(aggregateId, "Account Name");
            aggregate.Deposit(10);
            aggregate.Deposit(15);
            aggregate.WithDrawFunds(5);

            await CommitChanges(aggregate);

            var actual = await _provider.GetEventsAsync(aggregate.GetType(), aggregateId);

            actual.Count().Should().Be(4);
        }

        [Fact]
        public async Task Commit_target_version_order_should_be_preserved()
        {
            var aggregateId = Guid.NewGuid();

            var aggregate = new BankAccount(aggregateId, "Account Name");
            aggregate.Deposit(10);
            aggregate.Deposit(15);
            aggregate.WithDrawFunds(5);

            await CommitChanges(aggregate);

            var actual = await _provider.GetEventsAsync(aggregate.GetType(), aggregateId);

            actual.Select(x => x.TargetVersion)
                .ToArray()
                .Should()
                .ContainInOrder(new[] { -1, 0, 1, 2 });
        }

        [Fact]
        public async Task Commit_should_support_committing_individual_events()
        {
            var aggregateId = Guid.NewGuid();

            var aggregate = new BankAccount(aggregateId, "Account Name");

            await CommitChanges(aggregate);

            aggregate.Deposit(10);

            await CommitChanges(aggregate);

            aggregate.Deposit(15);

            await CommitChanges(aggregate);

            aggregate.WithDrawFunds(5);

            await CommitChanges(aggregate);

            var actual = await _provider.GetEventsAsync(aggregate.GetType(), aggregateId);

            actual.Count().Should().Be(4);
        }

        private async Task CommitChanges(Aggregate aggregate)
        {
            await _provider.CommitChangesAsync(aggregate);
            //as we are not using the repo we need to remember to do this
            aggregate.MarkChangesAsCommitted();
        }
    }
}