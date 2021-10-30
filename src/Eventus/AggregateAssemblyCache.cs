namespace Eventus
{
    using System.Collections.Generic;
    using System.Reflection;

    public class AggregateAssemblyCache
    {
        public AggregateAssemblyCache(List<Assembly>? aggregateAssemblies)
        {
            AggregateAssemblies = aggregateAssemblies;
        }
        
        public static List<Assembly>? AggregateAssemblies { get; set; }
    }
}