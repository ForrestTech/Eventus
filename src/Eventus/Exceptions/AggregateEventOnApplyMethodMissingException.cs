using System;

namespace Eventus.Exceptions
{
    public class AggregateEventOnApplyMethodMissingException : Exception
    {
        public AggregateEventOnApplyMethodMissingException(string msg) : base(msg)
        {

        }
    }
}