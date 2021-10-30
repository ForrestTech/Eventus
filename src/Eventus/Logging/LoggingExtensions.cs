namespace Eventus.Logging
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;

    internal static class LoggingExtensions
    {
        [DebuggerStepThrough]
        internal static void Enter(this ILogger logger, string type, object? parameters = null,
            [CallerMemberName] string methodName = "")
        {
            logger.LogTrace("Entering '{Method}' of '{Type}', arguments: '{@Parameters}'", methodName, type, parameters);
        }

        [DebuggerStepThrough]
        internal static void Exit(this ILogger logger, string type, object? parameters = null, object? result = null,
            [CallerMemberName] string methodName = "")
        {
            logger.LogTrace("Exiting '{Method}' of '{Type}', arguments: '{@Parameters}', result: '{@Result}'", methodName,
                type, parameters, result);
        }

        [DebuggerStepThrough]
        internal static void Exception(this ILogger logger, string type, Exception ex,
            [CallerMemberName] string methodName = "")
        {
            logger.LogError(ex, "Exception thrown when executing '{Method}' of '{Type}', Exception: '{@Ex}'", methodName,
                type, ex);
        }
    }
}