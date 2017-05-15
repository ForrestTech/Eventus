using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.Events;
using EventSourcing.Samples.Core.Domain;
using EventSourcing.Samples.Core.Events;
using FluentAssertions;
using Xunit;

namespace EventSourcing.Tests.Unit
{
    public class AggregateTests
    {
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
    }
}