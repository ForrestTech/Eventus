using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eventus.Domain;
using Eventus.Events;
using Eventus.Samples.Core.Domain;
using Xunit;

namespace Eventus.Tests.Unit
{
    public class ValidateAggregates
    {
        [Fact]
        public void Foo()
        {
            //validate that every event has an application method in an aggregate

            //get all domain events
            var domainAssembly = Assembly.GetAssembly(typeof(BankAccount));

            var eventType = typeof(Event);
            var events = domainAssembly.GetTypes()
                .Where(t => t != eventType && eventType.IsAssignableFrom(t));

            //get all aggregates
            var aggregateType = typeof(Aggregate);
            var aggregates = domainAssembly.GetTypes()
                .Where(t => t != aggregateType && aggregateType.IsAssignableFrom(t));

            //get aggregate apply methods
            var aggregateMethods = aggregates.SelectMany(a => a.GetMethodsBySig(typeof(void), true, typeof(IEvent))).Select(m => m.GetParameters().First());

            var errors = new List<string>();

            foreach (var @event in events)
            {
                var noMethodForType = aggregateMethods.All(m => m.ParameterType != @event);

                if (noMethodForType)
                {
                    errors.Add($"No method found to handle the event {@event.Name}");
                }
            }

            if (errors.Any())
            {
                throw new Exception("Some domain events don't have a method on any aggregate");
            }
        }
    }
}