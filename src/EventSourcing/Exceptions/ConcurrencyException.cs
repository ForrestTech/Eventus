using System;

namespace EventSourcing.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(Guid correlationId) 
            : base($"Aggregate {correlationId} has been modified externally and has an updated state. Can't commit changes.")
        {

        }
    }
}