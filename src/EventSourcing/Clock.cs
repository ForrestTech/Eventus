using System;

namespace EventSourcing
{
    public static class Clock
    {
        public static Func<DateTime> Now = () => DateTime.UtcNow;
    }
}