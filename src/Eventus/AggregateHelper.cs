namespace Eventus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    
    public static class AggregateHelper
    {
        public static IEnumerable<Type> GetAggregateTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var aggregateType = typeof(Aggregate);
            var types = assemblies.SelectMany(t => t.GetTypes())
                .Where(t => t != aggregateType && aggregateType.IsAssignableFrom(t));

            return types;
        }
    }
}