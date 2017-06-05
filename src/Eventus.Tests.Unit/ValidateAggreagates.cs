using System;
using System.Reflection;
using Eventus.Events;
using Eventus.Exceptions;
using Eventus.Samples.Core.Domain;
using Eventus.Test;
using FluentAssertions;
using Xunit;

namespace Eventus.Tests.Unit
{
    public class ValidateAggregatesTests
    {
        [Fact]
        public void Validate_that_aggregates_support_all_events()
        {
            ValidateAggregates.AssertThatAggregatesSupportAllEvents(Assembly.GetAssembly(typeof(BankAccount)));
        }

        [Fact]
        public void Validate_that_events_without_methods_throw_exception()
        {
            Action act = () => ValidateAggregates.AssertThatAggregatesSupportAllEvents(Assembly.GetAssembly(typeof(EventWithoutMethod)), Assembly.GetAssembly(typeof(BankAccount)));

            act.ShouldThrow<AggregateEventNotSupportException>()
                .Where(e => e.Message.Contains("EventWithoutMethod"));
        }

        public class EventWithoutMethod : Event
        { }
    }
}