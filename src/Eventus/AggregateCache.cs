namespace Eventus
{
    using Domain;
    using Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class AggregateCache
    {
        private static List<Assembly>? _internalAggregateAssemblies;
        private static Dictionary<Type, Dictionary<Type, MethodInfo>>? _internalAggregateEventHandlerCache;

        private static Dictionary<Type, Dictionary<Type, MethodInfo>> AggregateEventHandlerCache
        {
            get
            {
                _internalAggregateEventHandlerCache ??= BuildAggregateEventHandlerCache();
                return _internalAggregateEventHandlerCache;
            }
        }

        public static List<Assembly> AggregateAssemblies
        {
            get
            {
                if (_internalAggregateAssemblies != null)
                {
                    return _internalAggregateAssemblies;
                }

                _internalAggregateAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                return _internalAggregateAssemblies;
            }
            set
            {
                _internalAggregateAssemblies = value;
            }
        }

        public static Dictionary<Type, MethodInfo> GetEventHandlersForAggregate(Type aggregateType)
        {
            return AggregateEventHandlerCache[aggregateType];
        }
        
        public static List<Type> GetAggregateTypes()
        {
            var aggregateType = typeof(Aggregate);
            var assemblies = AggregateAssemblies;
            var types = assemblies.SelectMany(t => t.GetTypes())
                .Where(t => t != aggregateType && aggregateType.IsAssignableFrom(t))
                .ToList();

            return types;
        }

        private static Dictionary<Type, Dictionary<Type, MethodInfo>> BuildAggregateEventHandlerCache()
        {
            var cache = new Dictionary<Type, Dictionary<Type, MethodInfo>>();
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
                
                cache.Add(aggregateType, eventHandlers);
            }

            return cache;
        }
    }
}