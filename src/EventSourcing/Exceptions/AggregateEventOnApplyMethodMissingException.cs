using System;

namespace EventSourcing.Exceptions
{
    public class AggregateEventOnApplyMethodMissingException : Exception
    {
        public AggregateEventOnApplyMethodMissingException(string msg) : base(msg)
        {

        }
    }
}