namespace Eventus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Events;
    using Exceptions;
    using System.Reflection;

    public static class AggregateHelper
    {
        public static IEnumerable<Type> GetAggregateTypes(IEnumerable<Assembly> aggregateAssemblies)
        {
            var aggregateType = typeof(Aggregate);
            var types = aggregateAssemblies.SelectMany(t => t.GetTypes())
                .Where(t => t != aggregateType && aggregateType.IsAssignableFrom(t));

            return types;
        }
        
        public static void AssertThatAggregatesSupportAllEvents(params Assembly[] aggregateAssemblies)
        {
            AssertThatAggregatesSupportAllEvents(aggregateAssemblies.ToList());
        }

        public static void AssertThatAggregatesSupportAllEvents(Assembly aggregateAssembly)
        {
            AssertThatAggregatesSupportAllEvents(new List<Assembly>
            {
                aggregateAssembly
            });
        }
        
        public static void AssertThatAggregatesSupportAllEvents(List<Assembly> aggregateAssemblies)
        {
            var eventType = typeof(Event);
            var events = aggregateAssemblies.SelectMany(a => a.GetTypes())
                .Where(t => t != eventType && eventType.IsAssignableFrom(t));

            //get all aggregates
            var aggregateType = typeof(Aggregate);
            var aggregates = aggregateAssemblies.SelectMany(a => a.GetTypes())
                .Where(t => t != aggregateType && aggregateType.IsAssignableFrom(t));

            //get aggregate apply methods
            var aggregateMethods = aggregates.SelectMany(a => a.GetMethodsBySig(typeof(void), true, typeof(IEvent)))
                .Select(m => m.GetParameters().First())
                .ToList();

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