using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eventus.Domain;
using Eventus.Events;
using Eventus.Exceptions;

namespace Eventus.Test
{
    public static class ValidateAggregates
    {
        public static void AssertThatAggregatesSupportAllEvents(params Assembly[] domainAssemblies)
        {
            //get all events in all assemlies
            var eventType = typeof(Event);
            var events = domainAssemblies.SelectMany(a => a.GetTypes())
                .Where(t => t != eventType && eventType.IsAssignableFrom(t));

            //get all aggregates
            var aggregateType = typeof(Aggregate);
            var aggregates = domainAssemblies.SelectMany(a => a.GetTypes())
                .Where(t => t != aggregateType && aggregateType.IsAssignableFrom(t));

            //get aggregate apply methods
            var aggregateMethods = aggregates.SelectMany(a => a.GetMethodsBySig(typeof(void), true, typeof(IEvent)))
                .Select(m => m.GetParameters().First());

            var errors = new List<Type>();

            foreach (var @event in events)
            {
                var noMethodForType = aggregateMethods.All(m => m.ParameterType != @event);

                if (noMethodForType)
                {
                    errors.Add(@event);
                }
            }

            if (errors.Any())
            {
                throw new AggregateEventNotSupportException(errors);
            }
        }
    }
}