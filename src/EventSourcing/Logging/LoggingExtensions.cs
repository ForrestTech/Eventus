using System;
using System.Runtime.CompilerServices;

namespace EventSourcing.Logging
{
    internal static class LoggingExtensions
    {
        internal static void Enter(this ILog logger, string type, object argument = null, [CallerMemberName]string method = "")
        {
            logger.InfoFormat("Entering {method} of {type} , arguments {@argument}", method, type, argument);
        }

        internal static void Exit(this ILog logger, string type, [CallerMemberName]string method = "")
        {
            logger.InfoFormat("Exiting {method} of {type}", type, method);
        }

        internal static void Exit(this ILog logger, string type, object result, [CallerMemberName]string method = "")
        {
            logger.InfoFormat("Exiting {method} of {type}, result: {@result} ", method, type, result);
        }

        internal static void Exception(this ILog logger, string type, Exception ex, [CallerMemberName]string method = "")
        {
            logger.InfoFormat("Exception thrown when executing {method} of {type}, Exception: {@ex} ", method, type, ex);
        }
    }
}