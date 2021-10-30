namespace Eventus.UnitTests
{
    using Configuration;
    using FluentAssertions;
    using Samples.Core.Domain;
    using Storage;
    using System;
    using Xunit;

    public class SnapshotCalculatorTests
    {
        [Theory]
        [InlineData(0, 4, 3, true)]
        [InlineData(0, 3, 3, true)]
        [InlineData(0, 2, 3, false)]
        [InlineData(3, 4, 3, true)]
        [InlineData(3, 3, 3, true)]
        [InlineData(3, 2, 3, false)]
        [InlineData(5, 1, 3, true)]
        [InlineData(5, 2, 3, true)]
        [InlineData(5, 4, 3, true)]
        [InlineData(4, 1, 3, false)]
        public void ShouldCreateSnapShot(int committedChanges, int uncommittedChanges, int snapShotFrequency, bool shouldTakeSnapshot)
        {
            var aggregate = CreateAggregateWithUncommittedChanges(committedChanges, uncommittedChanges);

            var options = new EventusOptions {SnapshotFrequency = snapShotFrequency};

            var sut = new SnapshotCalculator(options);

            var actual = sut.ShouldCreateSnapShot(aggregate);

            actual.Should().Be(shouldTakeSnapshot);
        }

        private static BankAccount CreateAggregateWithUncommittedChanges(int committedChanges, int numberOfUncommittedChanges)
        {
            var aggregate = new BankAccount(Guid.NewGuid(), "Joe Bloggs");

            for (int i = 1; i <= committedChanges - 1; i++)
            {
                //Deposit are safe uncommitted changes
                aggregate.Deposit(10, Guid.NewGuid());
            }
            
            aggregate.MarkChangesAsCommitted();

            // if we have no committed changes then this number is one lower as account creation counts as an change 
            if (committedChanges == 0)
            {
                numberOfUncommittedChanges -= 1;
            }

            for (int i = 1; i <= numberOfUncommittedChanges; i++)
            {
                //Deposit are safe uncommitted changes
                aggregate.Deposit(10, Guid.NewGuid());
            }

            return aggregate;
        }
    }
}