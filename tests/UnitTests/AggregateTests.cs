namespace Eventus.UnitTests
{
    using Domain;
    using Events;
    using Exceptions;
    using FluentAssertions;
    using MassTransit;
    using Samples.Core.Domain;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class AggregateTests
    {
        [Fact]
        public void Aggregates_are_equal_if_there_ids_match()
        {
            var accountId = NewId.NextGuid();

            var first = new BankAccount(accountId, "Jim");
            var second = new BankAccount(accountId, "Kate");

            first.Should().Be(second);
        }

        [Fact]
        public void Aggregates_are_not_equal_if_there_ids_dont_match()
        {
            var first = new BankAccount("Jim");
            var second = new BankAccount("Kate");

            first.Should().NotBe(second);
        }

        [Fact]
        public void Aggregate_should_have_uncommitted_changes_when_an_event_is_applied()
        {
            var aggregate = new BankAccount("Joe Bloggs");

            aggregate.HasUncommittedChanges()
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Aggregate_should_add_events_to_uncommitted_changes()
        {
            var aggregate = new BankAccount("Joe Bloggs");

            aggregate.GetUncommittedChanges()
                .Should()
                .HaveCount(1);
        }

        [Fact]
        public void Aggregate_should_add_new_events_to_uncommitted_changes()
        {
            var aggregate = new BankAccount("Joe Bloggs");
            var uncommittedCount = aggregate.GetUncommittedChanges().Count();

            aggregate.Deposit(100);

            uncommittedCount.Should().Be(1);
            aggregate.GetUncommittedChanges().Count().Should().Be(2);
        }

        [Fact]
        public void Aggregate_should_add_correct_event_to_uncommitted_changes()
        {
            var accountName = "Joe Bloggs";

            var aggregate = new BankAccount(accountName);

            aggregate.GetUncommittedChanges()
                .First()
                .Should()
                .BeOfType(typeof(AccountCreatedEvent));
        }

        [Fact]
        public void Aggregate_should_update_version_for_each_event_applied()
        {
            var aggregate = new BankAccount("Joe Bloggs");
            var version = aggregate.CurrentVersion;

            aggregate.Deposit(100);

            version.Should().Be(1);
            aggregate.CurrentVersion.Should().Be(2);
        }

        [Fact]
        public void Aggregate_last_committed_version_starts_at_zero()
        {
            var aggregate = new BankAccount("Joe Bloggs");
            var lastCommittedVersion = aggregate.LastCommittedVersion;

            lastCommittedVersion.Should().Be(0);
        }

        [Fact]
        public void Aggregate_last_committed_version_updates_to_current_version_when_changes_committed()
        {
            var aggregate = new BankAccount("Joe Bloggs");

            aggregate.MarkChangesAsCommitted();

            aggregate.LastCommittedVersion.Should().Be(1);
        }

        [Fact]
        public void Aggregate_cant_have_invalid_events_applied()
        {
            var aggregate = new BankAccount("Joe Bloggs");

            aggregate.Invoking(x => x.LoadFromHistory(new List<IEvent> { new EventWithoutMatchingAggregate() }))
                .Should().Throw<AggregateEventOnApplyMethodMissingException>();
        }


        [Fact]
        public void Aggregate_cant_have_invalid_events_applied_when_there_is_no_apply_method()
        {
            var aggregate = new TestAggregate();

            aggregate.Invoking(x => x.LoadFromHistory(new List<IEvent> { new TestAggregateEvent() }))
                .Should().Throw<AggregateEventOnApplyMethodMissingException>();
        }

        private class EventWithoutMatchingAggregate : Event
        { }

        private class TestAggregate : Aggregate
        {}

        private class TestAggregateEvent : Event
        { }
    }
}