namespace Eventus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Events;

    internal static class ReflectionHelper
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, string>> AggregateEventHandlerCache =
                new();

        public static Dictionary<Type, string> FindEventHandlerMethodsInAggregate(Type aggregateType)
        {
            if (AggregateEventHandlerCache.ContainsKey(aggregateType) == false)
            {
                var eventHandlers = new ConcurrentDictionary<Type, string>();

                var methods = aggregateType.GetMethodsBySig(typeof(void), true, typeof(IEvent)).ToList();

                if (methods.Any())
                {
                    foreach (var m in methods)
                    {
                        var parameter = m.GetParameters().First();
                        if (eventHandlers.TryAdd(parameter.ParameterType, m.Name) == false)
                        {
                            throw new TargetException($"Multiple methods found handling same event in {aggregateType.Name}");
                        }
                    }
                }

                if (AggregateEventHandlerCache.TryAdd(aggregateType, eventHandlers) == false)
                {
                    throw new TargetException($"Error registering methods for handling events in {aggregateType.Name}");
                }
            }


            return AggregateEventHandlerCache[aggregateType].ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static MethodInfo? GetMethod(Type t, string methodName, Type[] paramTypes)
        {
            return t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);
        }

        public static IEnumerable<MethodInfo> GetMethodsBySig(this Type type,
            Type returnType,
            bool matchParameterInheritance,
            params Type[] parameterTypes)
        {
            return type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where((m) =>
            {
                //ignore properties
                if (m.Name.StartsWith("get_", StringComparison.InvariantCultureIgnoreCase) ||
                    m.Name.StartsWith("set_", StringComparison.InvariantCultureIgnoreCase))
                    return false;

                if (m.ReturnType != returnType)
                    return false;

                var parameters = m.GetParameters();

                if (parameterTypes.Length == 0)
                    return parameters.Length == 0;

                if (parameters.Length != parameterTypes.Length)
                    return false;

                return !parameterTypes.Where((t, i) => (parameters[i].ParameterType == t || matchParameterInheritance && t.IsAssignableFrom(parameters[i].ParameterType)) == false).Any();
            });
        }
    }
}