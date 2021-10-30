namespace Eventus.Logging
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;

    public abstract class LoggingDecorator
    {
        private readonly ILogger _logger;

        protected LoggingDecorator(ILogger logger)
        {
            _logger = logger;
        }

        [DebuggerStepThrough]
        protected async Task<TResult?> LogMethodCallAsync<TResult>(string typename, Func<Task<TResult>> method,
            object parameter, [CallerMemberName] string methodName = "")
        {
            try
            {
                _logger.Enter(typename, parameter, methodName);

                var result = await method();

                _logger.Exit(typename, parameter, result, methodName);

                return result;
            }
            catch (Exception e)
            {
                _logger.Exception(typename, e, methodName);
                throw;
            }
        }

        [DebuggerStepThrough]
        protected async Task<TResult> LogMethodCallAsync<TResult>(string typename, Func<Task<TResult>> method,
            object[] parameter, [CallerMemberName] string methodName = "")
        {
            try
            {
                _logger.Enter(typename, parameter, methodName);

                var result = await method();

                _logger.Exit(typename, parameter, result, methodName);

                return result;
            }
            catch (Exception e)
            {
                _logger.Exception(typename, e);
                throw;
            }
        }

        [DebuggerStepThrough]
        protected Task LogMethodCallAsync(string typename, Func<Task> method, object parameter,
            [CallerMemberName] string methodName = "")
        {
            try
            {
                _logger.Enter(typename, parameter, methodName);

                var result = method();

                _logger.Exit(typename, parameter, methodName: methodName);

                return result;
            }
            catch (Exception e)
            {
                _logger.Exception(typename, e, methodName);
                throw;
            }
        }
    }
}