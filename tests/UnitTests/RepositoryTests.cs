namespace Eventus.UnitTests
{
    using AutoFixture.Xunit2;
    using Configuration;
    using Domain;
    using EventBus;
    using Events;
    using Exceptions;
    using Fixture;
    using FluentAssertions;
    using Moq;
    using Samples.Core.Domain;
    using Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class RepositoryTests
    {
        [Theory, AutoMoqData]
        public async Task Get_should_try_to_get_latest_snapshot_if_aggregate_is_snapshotable(
            [Frozen] Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo,
            Guid aggregateId)
        {
            snapshotProvider.Setup(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId)))
                .Returns(Task.FromResult<Snapshot>(null));

            await repo.GetByIdAsync<BankAccount>(aggregateId);

            snapshotProvider.Verify(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId)),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_not_try_to_get_latest_snapshot_if_aggregate_is_not_snapshotable(
            [Frozen] Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo,
            Guid aggregateId)
        {
            await repo.GetByIdAsync<NoneSnapshotable>(aggregateId);

            snapshotProvider.Verify(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(NoneSnapshotable)),
                    It.Is<Guid>(g => g == aggregateId)),
                Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_get_all_events_if_no_snapshot_is_returned(
            [Frozen] Mock<ISnapshotStorageProvider> snapshotProvider,
            [Frozen] Mock<IEventStorageProvider> eventProvider,
            Repository repo,
            Guid aggregateId)
        {
            snapshotProvider.Setup(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId)))
                .Returns(Task.FromResult<Snapshot>(null));

            await repo.GetByIdAsync<BankAccount>(aggregateId);

            eventProvider.Verify(x => x.GetEventsAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId),
                    It.Is<int>(s => s == 0),
                    It.Is<int>(c => c == int.MaxValue)),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_get_all_events_after_snapshot_if_one_is_returned(
            [Frozen] Mock<ISnapshotStorageProvider> snapshotProvider,
            [Frozen] Mock<IEventStorageProvider> eventProvider,
            Repository repo,
            BankAccountSnapshot snapshot,
            Guid aggregateId)
        {
            snapshotProvider.Setup(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId)))
                .Returns(Task.FromResult<Snapshot>(snapshot));

            await repo.GetByIdAsync<BankAccount>(aggregateId);

            eventProvider.Verify(x => x.GetEventsAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId),
                    It.Is<int>(s => s == snapshot.Version + 1),
                    It.Is<int>(c => c == int.MaxValue)),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_return_aggregate_loaded_from_events(
            [Frozen] Mock<IEventStorageProvider> eventProvider,
            [Frozen] Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo,
            Guid aggregateId)
        {
            snapshotProvider.Setup(x => x.GetSnapshotAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId)))
                .Returns(Task.FromResult<Snapshot>(null));

            eventProvider.Setup(x => x.GetEventsAsync(
                    It.Is<Type>(t => t == typeof(BankAccount)),
                    It.Is<Guid>(g => g == aggregateId),
                    It.Is<int>(s => s == 0),
                    It.Is<int>(c => c == int.MaxValue)))
                .Returns(Task.FromResult<IEnumerable<IEvent>>(new List<IEvent>
                {
                    new AccountCreatedEvent(aggregateId, 0, Guid.NewGuid(), "Joe Bloggs"),
                    new FundsDepositedEvent(aggregateId, 1, Guid.NewGuid(), 10),
                    new FundsWithdrawalEvent(aggregateId, 2, Guid.NewGuid(), 5)
                }));

            var actual = await repo.GetByIdAsync<BankAccount>(aggregateId);

            actual.Should().NotBeNull();
            actual?.Id.Should().Be(aggregateId);
            actual?.CurrentBalance.Should().Be(5);
        }

        [Theory, AutoMoqData]
        public async Task Get_should_return_aggregate_loaded_from_events_and_snapshot(
            [Frozen] Mock<ISnapshotStorageProvider> snapshotProvider,
            [Frozen] Mock<IEventStorageProvider> eventProvider,
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
                    new FundsDepositedEvent(aggregateId, 1, Guid.NewGuid(), 10),
                    new FundsWithdrawalEvent(aggregateId, 2, Guid.NewGuid(), 5)
                }));

            var actual = await repo.GetByIdAsync<BankAccount>(aggregateId);

            actual.Should().NotBeNull();
            actual?.Id.Should().Be(aggregateId);
            actual?.CurrentBalance.Should().Be(15);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_commits_nothing_when_there_are_no_uncommitted_changes(
            [Frozen] Mock<IEventStorageProvider> eventStore,
            Repository repo,
            BankAccount aggregate)
        {
            await repo.SaveAsync(aggregate);

            eventStore.Verify(x => x.CommitChangesAsync(aggregate), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_gets_last_event_to_check_target_version(
            [Frozen] Mock<IEventStorageProvider> eventStore,
            Repository repo,
            Guid aggregateId)
        {
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");

            eventStore.Setup(x =>
                    x.GetLastEventAsync(It.Is<Type>(t => t == aggregate.GetType()), It.Is<Guid>(i => i == aggregateId)))
                .Returns(Task.FromResult<IEvent>(null))
                .Verifiable();

            await repo.SaveAsync(aggregate);

            eventStore.VerifyAll();
        }

        [Theory, AutoMoqData]
        public void Save_should_throws_when_aggregate_exist_for_expected_version_of(
            [Frozen] Mock<IEventStorageProvider> eventStore,
            Repository repo,
            AccountCreatedEvent existingAccount,
            Guid aggregateId)
        {
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");

            eventStore.Setup(x =>
                    x.GetLastEventAsync(It.Is<Type>(t => t == aggregate.GetType()), It.Is<Guid>(i => i == aggregateId)))
                .Returns(Task.FromResult<IEvent>(existingAccount))
                .Verifiable();

            Action act = () => repo.SaveAsync(aggregate).Wait();

            act.Should().Throw<AggregateException>()
                .WithInnerException<AggregateCreationException>();
        }

        [Theory, AutoMoqData]
        public void Save_should_throws_when_aggregate_target_version_does_not_match_last_event(
            [Frozen] Mock<IEventStorageProvider> eventStore,
            Repository repo,
            AccountCreatedEvent existingAccount,
            Guid aggregateId)
        {
            //create an aggregate with history then update it
            var aggregate = new BankAccount();
            aggregate.LoadFromHistory(
                new List<IEvent> {new AccountCreatedEvent(aggregateId, 0, Guid.NewGuid(), "Jeff")});
            aggregate.Deposit(100);

            //force the last event stored to be newer than expected (the first event should target 0)
            existingAccount.TargetVersion = 1; 
            eventStore.Setup(x =>
                    x.GetLastEventAsync(It.Is<Type>(t => t == aggregate.GetType()), It.Is<Guid>(i => i == aggregateId)))
                .Returns(Task.FromResult<IEvent>(existingAccount))
                .Verifiable();

            Action act = () => repo.SaveAsync(aggregate).Wait();

            act.Should().Throw<AggregateException>()
                .WithInnerException<ConcurrencyException>();
        }

        [Theory, AutoMoqData]
        public async Task Save_should_call_commit_for_a_valid_aggregate([Frozen] Mock<IEventStorageProvider> eventStore,
            Repository repo)
        {
            SetupNoExistingEvents(eventStore);

            var aggregate = CreateAggregateWithUncommittedChanges(Guid.NewGuid());

            await repo.SaveAsync(aggregate);

            eventStore.Verify(x => x.CommitChangesAsync(It.Is<Aggregate>(a => a == aggregate)));
        }

        [Theory, AutoMoqData]
        public async Task Save_should_set_the_date_on_events_to_commit([Frozen] Mock<IEventStorageProvider> eventStore,
            Repository repo,
            DateTime now)
        {
            SetupNoExistingEvents(eventStore);

            var aggregate = CreateAggregateWithUncommittedChanges(Guid.NewGuid());

            Clock.Now = () => now;

            await repo.SaveAsync(aggregate);

            eventStore.Verify(x => x.CommitChangesAsync(
                    It.Is<Aggregate>(a => a == aggregate &&
                                          aggregate.GetUncommittedChanges()
                                              .All(e => e.EventCommittedTimestamp == now))),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_publish_each_event_in_aggregate([Frozen] Mock<IEventStorageProvider> eventStore,
            [Frozen] Mock<IEventPublisher> publisher,
            Repository repo)
        {
            SetupNoExistingEvents(eventStore);

            var aggregate = CreateAggregateWithUncommittedChanges(Guid.NewGuid());

            await repo.SaveAsync(aggregate);

            publisher.Verify(x => x.PublishAsync(It.IsAny<IEvent>()), Times.Exactly(3));
        }

        [Theory, AutoMoqData]
        public async Task Save_should_not_store_snapshot_if_aggregate_is_not_snapshotable(
            [Frozen] Mock<IEventStorageProvider> eventStore,
            [Frozen] Mock<ISnapshotStorageProvider> snapshotProvider,
            Repository repo)
        {
            SetupNoExistingEvents(eventStore);

            var aggregate = new NoneSnapshotable("Test");

            await repo.SaveAsync(aggregate);

            snapshotProvider.Verify(x => x.SaveSnapshotAsync(It.IsAny<Type>(), It.IsAny<Snapshot>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_store_snapshot_if_aggregate_is_snapshotable(
            [Frozen] Mock<IEventStorageProvider> eventStore,
            [Frozen] Mock<ISnapshotStorageProvider> snapshotProvider,
            [Frozen] Mock<ISnapshotCalculator> snapshotCalculator,
            Repository repo)
        {
            SetupNoExistingEvents(eventStore);

            var aggregate = CreateAggregateWithUncommittedChanges(Guid.NewGuid());

            snapshotCalculator.Setup(x => x.ShouldCreateSnapShot(It.IsAny<BankAccount>()))
                .Returns(true);

            await repo.SaveAsync(aggregate);

            snapshotProvider.Verify(x => x.SaveSnapshotAsync(
                    It.Is<Type>(t => t == aggregate.GetType()),
                    It.Is<Snapshot>(s => s.GetType() == typeof(BankAccountSnapshot))),
                Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task Save_should_mark_all_changes_as_committed([Frozen] Mock<IEventStorageProvider> eventStore,
            Repository repo)
        {
            SetupNoExistingEvents(eventStore);

            var aggregate = CreateAggregateWithUncommittedChanges(Guid.NewGuid());

            await repo.SaveAsync(aggregate);

            aggregate.HasUncommittedChanges()
                .Should()
                .Be(false);
        }

        private static void SetupNoExistingEvents(Mock<IEventStorageProvider> eventStore)
        {
            eventStore.Setup(x => x.GetLastEventAsync(It.IsAny<Type>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult<IEvent>(null));
        }

        private static BankAccount CreateAggregateWithUncommittedChanges(Guid aggregateId)
        {
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");
            aggregate.Deposit(100);
            aggregate.Withdraw(10);
            return aggregate;
        }
    }
}