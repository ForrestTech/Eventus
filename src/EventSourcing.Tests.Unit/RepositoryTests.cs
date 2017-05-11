using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Domain;
using EventSourcing.Event;
using EventSourcing.EventBus;
using EventSourcing.Exceptions;
using EventSourcing.Samples.Core.Domain;
using EventSourcing.Samples.Core.Events;
using EventSourcing.Storage;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace EventSourcing.Tests.Unit
{
    public class RepositoryTests
    {
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
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");
            aggregate.Deposit(100);
            aggregate.WithDrawFunds(10);

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            eventStore.Verify(x => x.CommitChangesAsync(It.Is<Aggregate>(a => a == aggregate)));
        }

        [Theory, AutoMoqData]
        public async Task Save_should_set_the_date_on_events_to_commit([Frozen]Mock<IEventStorageProvider> eventStore,
            Repository repo,
            DateTime now,
            Guid aggregateId)
        {
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");
            aggregate.Deposit(100);
            aggregate.WithDrawFunds(10);

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
            var aggregate = new BankAccount(aggregateId, "Joe Bloggs");
            aggregate.Deposit(100);
            aggregate.WithDrawFunds(10);

            await repo.SaveAsync(aggregate).ConfigureAwait(false);

            publisher.Verify(x => x.PublishAsync(It.IsAny<IEvent>()), Times.Exactly(3));
        }
    }
}
