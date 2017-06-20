using System;
using System.Collections.Generic;
using Eventus.Samples.Core.Domain;
using FluentAssertions;
using Xunit;

namespace Eventus.Tests.Unit
{
    public class AggregateHelperTests
    {
        [Fact]
        public void GetAggregateTypes_should_return_all_aggregates()
        {
            var aggregates = AggregateHelper.GetAggregateTypes();

            aggregates.Should().Contain(new List<Type> {typeof(BankAccount)});
        }
    }
}