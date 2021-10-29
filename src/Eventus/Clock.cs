namespace Eventus
{
    using System;
    
    public static class Clock
    {
        public static Func<DateTime> Now = () => DateTime.UtcNow;
    }
}