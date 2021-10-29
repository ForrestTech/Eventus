namespace Eventus.Logging
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;

    internal static class LoggingExtensions
    {
        [DebuggerStepThrough]
        internal static void Enter(this ILogger logger, string type, object? argument = null, [CallerMemberName]string method = "")
        {
            logger.LogDebug("Entering '{Method}' of '{Type}', arguments: '{@Argument}'", method, type, argument);
        }

        [DebuggerStepThrough]
        internal static void Exit(this ILogger logger, string type, [CallerMemberName]string method = "")
        {
            logger.LogDebug("Exiting '{Method}' of '{Type}'", type, method);
        }

        [DebuggerStepThrough]
        internal static void Exit(this ILogger logger, string type, object? result, [CallerMemberName]string method = "")
        {
            logger.LogDebug("Exiting '{Method}' of '{Type}', result: '{@Result}'", method, type, result);
        }

        [DebuggerStepThrough]
        internal static void Exception(this ILogger logger, string type, Exception ex, [CallerMemberName]string method = "")
        {
            logger.LogError(ex, "Exception thrown when executing '{Method}' of '{Type}', Exception: '{@Ex}'", method, type, ex);
        }
    }
}