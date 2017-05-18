using System;

namespace Eventus
{
    public static class Clock
    {
        public static Func<DateTime> Now = () => DateTime.UtcNow;
    }
}