namespace Eventus.UnitTests
{
    using FluentAssertions;
    using Samples.Core.Domain;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class AggregateHelperTests
    {
        [Fact]
        public void GetAggregateTypes_should_return_all_aggregates()
        {
            var aggregates = AggregateCache.GetAggregateTypes();

            aggregates.Should().Contain(new List<Type> {typeof(BankAccount)});
        }
    }
}