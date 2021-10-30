namespace Eventus.UnitTests
{
    using Events;
    using Exceptions;
    using FluentAssertions;
    using Samples.Core.Domain;
    using System;
    using System.Reflection;
    using Xunit;

    public class ValidateAggregatesTests
    {
        [Fact]
        public void Validate_that_aggregates_support_all_events()
        {
            AggregateValidation.AssertThatAggregatesSupportAllEvents(Assembly.GetAssembly(typeof(BankAccount))!);
        }

        [Fact]
        public void Validate_that_events_without_methods_throw_exception()
        {
            Action act = () => AggregateValidation.AssertThatAggregatesSupportAllEvents(Assembly.GetAssembly(typeof(EventWithoutMethod))!, Assembly.GetAssembly(typeof(BankAccount))!);

            act.Should().Throw<AggregateEventNotSupportException>()
                .Where(e => e.Message.Contains("EventWithoutMethod"));
        }

        public class EventWithoutMethod : Event
        { }
    }
}