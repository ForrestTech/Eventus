using Eventus.Configuration;

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
        private readonly EventusOptions _options;

        protected LoggingDecorator(ILogger logger, EventusOptions options)
        {
            _logger = logger;
            _options = options;
        }

        [DebuggerStepThrough]
        protected async Task<TResult?> LogMethodCallAsync<TResult>(string typename, Func<Task<TResult>> method,
            object parameter, [CallerMemberName] string methodName = "")
        {
            try
            {
                _logger.Enter(typename, parameter, methodName);

                if (_options.DiagnosticTimingEnabled)
                {
                    return await Profile(typename, method, parameter, methodName);
                }

                var result = await method();

                _logger.Exit(typename, parameter, result, methodName: methodName);
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

                if (_options.DiagnosticTimingEnabled)
                {
                    return await Profile(typename, method, parameter, methodName);
                }

                var result = await method();

                _logger.Exit(typename, parameter, result, methodName: methodName);
                return result;
            }
            catch (Exception e)
            {
                _logger.Exception(typename, e);
                throw;
            }
        }

        [DebuggerStepThrough]
        protected async Task LogMethodCallAsync(string typename, Func<Task> method, object parameter,
            [CallerMemberName] string methodName = "")
        {
            try
            {
                _logger.Enter(typename, parameter, methodName);

                if (_options.DiagnosticTimingEnabled)
                {
                    await Profile(typename, method, parameter, methodName);
                    return;
                }

                await method();

                _logger.Exit(typename, parameter, methodName: methodName);

            }
            catch (Exception e)
            {
                _logger.Exception(typename, e, methodName);
                throw;
            }
        }

        private async Task<TResult> Profile<TResult>(string typename, Func<Task<TResult>> method, object parameter, string methodName)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await method();
            stopwatch.Stop();

            _logger.Exit(typename, parameter, result, stopwatch.Elapsed, methodName);
            return result;
        }
        
        private async Task Profile(string typename, Func<Task> method, object parameter, string methodName)
        {
            var stopwatch = Stopwatch.StartNew();
            await method();
            stopwatch.Stop();

            _logger.Exit(typename, parameter, executedIn:stopwatch.Elapsed, methodName: methodName);
        }
    }
}
