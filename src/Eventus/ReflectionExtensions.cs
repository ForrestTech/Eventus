namespace Eventus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class ReflectionExtensions
    {
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