namespace Eventus
{
    using Domain;
    using Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class AggregateCache
    {
        private static readonly Dictionary<Type, Dictionary<Type, MethodInfo>> AggregateEventHandlerCache = new();
        
        public AggregateCache(List<Assembly> aggregateAssemblies)
        {
            AggregateAssemblies = aggregateAssemblies;
            BuildAggregateEventHandlerCache();
        }

        public static List<Assembly> AggregateAssemblies { get; private set; } = AppDomain.CurrentDomain.GetAssemblies().ToList();

        public static Dictionary<Type, MethodInfo> GetEventHandlersForAggregate(Type aggregateType)
        {
            return AggregateEventHandlerCache[aggregateType];
        }
        
        public static List<Type> GetAggregateTypes()
        {
            var aggregateType = typeof(Aggregate);
            var types = AggregateAssemblies.SelectMany(t => t.GetTypes())
                .Where(t => t != aggregateType && aggregateType.IsAssignableFrom(t))
                .ToList();

            return types;
        }

        private static void BuildAggregateEventHandlerCache()
        {
            var aggregateTypes = GetAggregateTypes();

            foreach (Type aggregateType in aggregateTypes)
            {
                var eventHandlers = new Dictionary<Type, MethodInfo>();

                var methods = aggregateType.GetMethodsBySig(typeof(void), true, typeof(IEvent)).ToList();

                if (methods.Any())
                {
                    foreach (var m in methods)
                    {
                        var parameter = m.GetParameters().First();
                        eventHandlers.Add(parameter.ParameterType, m);
                    }
                }
                
                AggregateEventHandlerCache.Add(aggregateType, eventHandlers);
            }
        }
    }
}