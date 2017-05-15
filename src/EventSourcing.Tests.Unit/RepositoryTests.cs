using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Domain;
using EventSourcing.EventBus;
using EventSourcing.Events;
using EventSourcing.Exceptions;
using EventSourcing.Samples.Core.Domain;
using EventSourcing.Samples.Core.Events;
using EventSourcing.Storage;
using EventSourcing.Tests.Unit.Fixture;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace EventSourcing.Tests.Unit
{
    public class RepositoryTests
    {
        [Theory, AutoMoqData]
        public async Task Get_should_try_to_get_latest_snapshot_if_aggregate_is_snapshotable([Frozen]Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo,
            Guid aggregateId)
        {
            await repo.GetByIdAsync<BankAccount>(aggregateId)
                .ConfigureAwait(false);

            snapshotProvider.Verify(x => x.GetSnapshotAsync(
                It.Is<Type>(t => t == typeof(BankAccount)),
                It.Is<Guid>(g => g == aggregateId)),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_not_try_to_get_latest_snapshot_if_aggregate_is_not_snapshotable([Frozen]Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo,
            NoneSnapshotable noneSnapshotable,
            Guid aggregateId)
        {
            await repo.GetByIdAsync<NoneSnapshotable>(aggregateId)
                .ConfigureAwait(false);

            snapshotProvider.Verify(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(NoneSnapshotable)),
                    It.Is<Guid>(g => g == aggregateId)),
                Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_get_all_events_if_no_snapshot_is_returned([Frozen]Mock<ISnapshotStorageProvider> snapshotProvider,
            [Frozen]Mock<IEventStorageProvider> eventProvider,
            Repository repo,
            Guid aggregateId)
        {
            snapshotProvider.Setup(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId)))
                .Returns(Task.FromResult<Snapshot>(null));

            await repo.GetByIdAsync<BankAccount>(aggregateId)
                .ConfigureAwait(false);

            eventProvider.Verify(x => x.GetEventsAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId),
                    It.Is<int>(s => s == 0),
                    It.Is<int>(c => c == int.MaxValue)),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_get_all_events_after_snapshot_if_one_is_returned([Frozen]Mock<ISnapshotStorageProvider> snapshotProvider,
            [Frozen]Mock<IEventStorageProvider> eventProvider,
            Repository repo,
            BankAccountSnapshot snapshot,
            Guid aggregateId)
        {
            snapshotProvider.Setup(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId)))
                .Returns(Task.FromResult<Snapshot>(snapshot));

            await repo.GetByIdAsync<BankAccount>(aggregateId)
                .ConfigureAwait(false);

            eventProvider.Verify(x => x.GetEventsAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId),
                    It.Is<int>(s => s == snapshot.Version + 1),
                    It.Is<int>(c => c == int.MaxValue)),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_return_aggregate_loaded_from_events([Frozen]Mock<IEventStorageProvider> eventProvider,
            Repository repo,
            Guid aggregateId)
        {
            eventProvider.Setup(x => x.GetEventsAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId),
                    It.Is<int>(s => s == 0),
                    It.Is<int>(c => c == int.MaxValue)))
                .Returns(Task.FromResult<IEnumerable<IEvent>>(new List<IEvent>
                {
                    new AccountCreatedEvent(aggregateId,0,"Joe Bloggs"),
                    new FundsDepositedEvent(aggregateId,1,10),
                    new FundsWithdrawalEvent(aggregateId,2,5)
                }));

            var actual = await repo.GetByIdAsync<BankAccount>(aggregateId)
                .ConfigureAwait(false);

            actual.Should().NotBeNull();
            actual.Id.Should().Be(aggregateId);
            actual.CurrentBalance.Should().Be(5);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_return_aggregate_loaded_from_events_and_snapshot([Frozen]Mock<ISnapshotStorageProvider> snapshotProvider,
            [Frozen]Mock<IEventStorageProvider> eventProvider,
            List<Transaction> transactions,
            Repository repo,
            Guid aggregateId)
        {
            var snapshot = new BankAccountSnapshot(Guid.NewGuid(), aggregateId, 3, "Joe Bloggs", 10, transactions);

            snapshotProvider.Setup(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId)))
                .Returns(Task.FromResult<Snapshot>(snapshot));

            eventProvider.Setup(x => x.GetEventsAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId),
                    It.Is<int>(s => s == snapshot.Version + 1),
                    It.Is<int>(c => c == int.MaxValue)))
                .Returns(Task.FromResult<IEnumerable<IEvent>>(new List<IEvent>
                {
                    new FundsDepositedEvent(aggregateId,1,10),
                    new FundsWithdrawalEvent(aggregateId,2,5)
                }));

            var actual = await repo.GetByIdAsync<BankAccount>(aggregateId)
                .ConfigureAwait(false);

            actual.Should().NotBeNull();
            actual.Id.Should().Be(aggregateId);
            actual.CurrentBalance.Should().Be(15);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_commits_nothing_when_there_are_no_uncommitted_changes([Frozen]Mock<IEventStorageProvider> eventStore,
            Repository repo,
            BankAccount aggregate)
        {
            await repo.SaveAsync(aggregate)
                .ConfigureAwait(false);

            eventStore.Verify(x => x.CommitChangesAsync(aggregate), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_gets_last_event_to_check_target_version([Frozen]Mock<IEventStorageProvider> eventStore,
            Repository repo,
            Guid aggregateId)
        {
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");

            eventStore.Setup(x => x.GetLastEventAsync(It.Is<Type>(t => t == aggregate.GetType()), It.Is<Guid>(i => i == aggregateId)))
                .Returns(Task.FromResult<IEvent>(null))
                .Verifiable();

            await repo.SaveAsync(aggregate)
                .ConfigureAwait(false);

            eventStore.VerifyAll();
        }

        [Theory, AutoMoqData]
        public void Save_should_throws_when_aggregate_exist_for_expected_version_of([Frozen]Mock<IEventStorageProvider> eventStore,
            Repository repo,
            AccountCreatedEvent existingAccount,
            Guid aggregateId)
        {
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");

            eventStore.Setup(x => x.GetLastEventAsync(It.Is<Type>(t => t == aggregate.GetType()), It.Is<Guid>(i => i == aggregateId)))
                .Returns(Task.FromResult<IEvent>(existingAccount))
                .Verifiable();

            Action act = () => repo.SaveAsync(aggregate).Wait();

            act.ShouldThrow<AggregateException>()
                .WithInnerException<AggregateCreationException>();
        }

        [Theory, AutoMoqData]
        public void Save_should_throws_when_aggregate_target_version_does_not_match_last_event([Frozen]Mock<IEventStorageProvider> eventStore,
            Repository repo,
            AccountCreatedEvent existingAccount,
            Guid aggregateId)
        {
            //create an aggregate with history then update it
            var aggregate = new BankAccount();
            aggregate.LoadFromHistory(new List<IEvent>
            {
                new AccountCreatedEvent(aggregateId, 0, "Jeff")
            });
            aggregate.Deposit(100);

            existingAccount.TargetVersion = 0;//force the last event stored to be newer than expected (the first event should target -1)
            eventStore.Setup(x => x.GetLastEventAsync(It.Is<Type>(t => t == aggregate.GetType()), It.Is<Guid>(i => i == aggregateId)))
                .Returns(Task.FromResult<IEvent>(existingAccount))
                .Verifiable();

            Action act = () => repo.SaveAsync(aggregate).Wait();

            act.ShouldThrow<AggregateException>()
                .WithInnerException<ConcurrencyException>();
        }

        [Theory, AutoMoqData]
        public async Task Save_should_call_commit_for_a_valid_aggregate([Frozen]Mock<IEventStorageProvider> eventStore,
            Repository repo,
            Guid aggregateId)
        {
            var aggregate = CreateAggregateWithUncommittedChanges(aggregateId);

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            eventStore.Verify(x => x.CommitChangesAsync(It.Is<Aggregate>(a => a == aggregate)));
        }

        [Theory, AutoMoqData]
        public async Task Save_should_set_the_date_on_events_to_commit([Frozen]Mock<IEventStorageProvider> eventStore,
            Repository repo,
            DateTime now,
            Guid aggregateId)
        {
            var aggregate = CreateAggregateWithUncommittedChanges(aggregateId);

            Clock.Now = () => now;

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            eventStore.Verify(x => x.CommitChangesAsync(
                It.Is<Aggregate>(a => a == aggregate &&
                aggregate.GetUncommittedChanges().All(e => e.EventCommittedTimestamp == now))),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_publish_each_event_in_aggregate([Frozen]Mock<IEventPublisher> publisher,
            Repository repo,
            Guid aggregateId)
        {
            var aggregate = CreateAggregateWithUncommittedChanges(aggregateId);

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            publisher.Verify(x => x.PublishAsync(It.IsAny<IEvent>()), Times.Exactly(3));
        }

        [Theory, AutoMoqData]
        public async Task Save_should_not_store_snapshot_if_aggregate_is_not_snapshotable([Frozen]Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo,
            Guid aggregateId)
        {
            var aggregate = new NoneSnapshotable("Test");

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            snapshotProvider.Verify(x => x.SaveSnapshotAsync(It.IsAny<Type>(), It.IsAny<Snapshot>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_not_check_snapshot_frequency_if_aggregate_is_not_snapshotable_([Frozen]Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo,
            Guid aggregateId)
        {
            var aggregate = new NoneSnapshotable("Test");

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            snapshotProvider.Verify(x => x.SnapshotFrequency, Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_store_snapshot_if_aggregate_is_snapshotable([Frozen]Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo,
            Guid aggregateId)
        {
            var aggregate = CreateAggregateWithUncommittedChanges(aggregateId);

            snapshotProvider.Setup(x => x.SnapshotFrequency).Returns(1);

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            snapshotProvider.Verify(x => x.SaveSnapshotAsync(
                It.Is<Type>(t => t == aggregate.GetType()),
                It.Is<Snapshot>(s => s.GetType() == typeof(BankAccountSnapshot))),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_not_store_snapshot_if_changes_to_commit_greater_than_frequency([Frozen]Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo,
            Guid aggregateId)
        {
            //3  uncommitted changes
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");
            aggregate.Deposit(100);
            aggregate.WithDrawFunds(10);

            //frequency is greater than uncommitted changes
            snapshotProvider.Setup(x => x.SnapshotFrequency).Returns(4);

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            snapshotProvider.Verify(x => x.SaveSnapshotAsync(
                    It.Is<Type>(t => t == aggregate.GetType()),
                    It.Is<Snapshot>(s => s.GetType() == typeof(BankAccountSnapshot))),
                Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_mark_all_changes_as_committed(Repository repo,
            Guid aggregateId)
        {
            var aggregate = CreateAggregateWithUncommittedChanges(aggregateId);

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            aggregate.HasUncommittedChanges()
                .Should()
                .Be(false);
        }

        private static BankAccount CreateAggregateWithUncommittedChanges(Guid aggregateId)
        {
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");
            aggregate.Deposit(100);
            aggregate.WithDrawFunds(10);
            return aggregate;
        }
    }
}
