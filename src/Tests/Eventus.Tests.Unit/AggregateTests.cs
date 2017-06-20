using System;
using System.Collections.Generic;
using System.Linq;
using Eventus.Domain;
using Eventus.Events;
using Eventus.Exceptions;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.Core.Events;
using FluentAssertions;
using Xunit;

namespace Eventus.Tests.Unit
{
    public class AggregateTests
    {
        [Fact]
        public void Aggregates_are_equal_if_there_ids_match()
        {
            var accountId = Guid.NewGuid();

            var first = new BankAccount(accountId, "Jim");
            var second = new BankAccount(accountId, "Kate");

            first.Should().Be(second);
        }

        [Fact]
        public void Aggregates_are_not_equal_if_there_ids_dont_match()
        {
            var first = new BankAccount(Guid.NewGuid(), "Jim");
            var second = new BankAccount(Guid.NewGuid(), "Kate");

            first.Should().NotBe(second);
        }

        [Fact]
        public void Aggregate_should_have_uncommitted_changes_when_an_event_is_applied()
        {
            var aggregate = new BankAccount(Guid.NewGuid(), "Joe Bloggs");

            aggregate.HasUncommittedChanges()
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Aggregate_should_add_events_to_uncommitted_changes()
        {
            var aggregate = new BankAccount(Guid.NewGuid(), "Joe Bloggs");

            aggregate.GetUncommittedChanges()
                .Should()
                .HaveCount(1);
        }

        [Fact]
        public void Aggregate_should_add_new_events_to_uncommitted_changes()
        {
            var aggregate = new BankAccount(Guid.NewGuid(), "Joe Bloggs");
            var uncommittedCount = aggregate.GetUncommittedChanges().Count();

            aggregate.Deposit(100);

            uncommittedCount.Should().Be(1);
            aggregate.GetUncommittedChanges().Count().Should().Be(2);
        }

        [Fact]
        public void Aggregate_should_add_correct_event_to_uncommitted_changes()
        {
            var aggregateId = Guid.NewGuid();
            var accountName = "Joe Bloggs";

            var aggregate = new BankAccount(aggregateId, accountName);

            aggregate.GetUncommittedChanges()
                .First()
                .Should()
                .BeOfType(typeof(AccountCreatedEvent));
        }

        [Fact]
        public void Aggregate_should_update_version_for_each_event_applied()
        {
            var aggregate = new BankAccount(Guid.NewGuid(), "Joe Bloggs");
            var version = aggregate.CurrentVersion;

            aggregate.Deposit(100);

            version.Should().Be(0);
            aggregate.CurrentVersion.Should().Be(1);
        }

        [Fact]
        public void Aggregate_last_committed_version_starts_at_minus_one()
        {
            var aggregate = new BankAccount(Guid.NewGuid(), "Joe Bloggs");
            var lastCommittedVersion = aggregate.LastCommittedVersion;

            lastCommittedVersion.Should().Be(-1);
        }

        [Fact]
        public void Aggregate_last_committed_version_updates_to_current_version_when_changes_committed()
        {
            var aggregate = new BankAccount(Guid.NewGuid(), "Joe Bloggs");

            aggregate.MarkChangesAsCommitted();

            aggregate.LastCommittedVersion.Should().Be(0);
        }

        [Fact]
        public void Aggregate_cant_have_invalid_events_applied()
        {
            var aggregate = new BankAccount(Guid.NewGuid(), "Joe Bloggs");

            aggregate.Invoking(x => x.LoadFromHistory(new List<IEvent> { new EventWithoutMatchingAggregate() }))
                .ShouldThrow<AggregateEventOnApplyMethodMissingException>();
        }


        [Fact]
        public void Aggregate_cant_have_invalid_events_applied_when_there_is_no_apply_method()
        {
            var aggregate = new TestAggregate();

            aggregate.Invoking(x => x.LoadFromHistory(new List<IEvent> { new TestAggregateEvent() }))
                .ShouldThrow<AggregateEventOnApplyMethodMissingException>();
        }

        private class EventWithoutMatchingAggregate : Event
        { }

        private class TestAggregate : Aggregate
        {}

        private class TestAggregateEvent : Event
        { }
    }
}