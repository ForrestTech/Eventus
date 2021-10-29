namespace Eventus.Logging
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Extensions.Logging;

    internal static class LoggingExtensions
    {
        internal static void Enter(this ILogger logger, string type, object? argument = null, [CallerMemberName]string method = "")
        {
            logger.LogDebug("Entering '{Method}' of '{Type}', arguments: '{@Argument}'", method, type, argument);
        }

        internal static void Exit(this ILogger logger, string type, [CallerMemberName]string method = "")
        {
            logger.LogDebug("Exiting '{Method}' of '{Type}'", type, method);
        }

        internal static void Exit(this ILogger logger, string type, object? result, [CallerMemberName]string method = "")
        {
            logger.LogDebug("Exiting '{Method}' of '{Type}', result: '{@Result}'", method, type, result);
        }

        internal static void Exception(this ILogger logger, string type, Exception ex, [CallerMemberName]string method = "")
        {
            logger.LogError(ex, "Exception thrown when executing '{Method}' of '{Type}', Exception: '{@Ex}'", method, type, ex);
        }
    }
}