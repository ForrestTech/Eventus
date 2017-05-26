using System;
using System.Collections.Generic;
using System.Linq;

namespace Eventus.Exceptions
{
    public class AggregateEventNotSupportException : Exception
    {
        public AggregateEventNotSupportException(IEnumerable<Type> eventsWithNoMethods)
            : base($@"Some domain events exists that don't have a application method on any aggregate. {Environment.NewLine}Events with no methods: {string.Join(",", eventsWithNoMethods.Select(t => t.FullName))}")
        { }
    }
}