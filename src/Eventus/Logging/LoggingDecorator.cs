using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Eventus.Logging
{
    public abstract class LoggingDecorator
    {
        private readonly ILogger _logger;

        protected LoggingDecorator(ILogger logger)
        {
            _logger = logger;
        }

        protected abstract string TypeName { get; }

        protected async Task<TResult?> LogMethodCallAsync<TResult>(Func<Task<TResult>> method, object parameter, [CallerMemberName] string methodName = "")
        {
            try
            {
                _logger.Enter(TypeName, parameter, methodName);

                var result = await method();

                _logger.Exit(TypeName, result, methodName);

                return result;
            }
            catch (Exception e)
            {
                _logger.Exception(TypeName, e, methodName);
                throw;
            }
        }

        protected async Task<TResult> LogMethodCallAsync<TResult>(Func<Task<TResult>> method, object[] parameter, [CallerMemberName] string methodName = "")
        {
            try
            {
                _logger.Enter(TypeName, parameter, methodName);

                var result = await method();

                _logger.Exit(TypeName, result, methodName);

                return result;
            }
            catch (Exception e)
            {
                _logger.Exception(TypeName, e);
                throw;
            }
        }

        protected Task LogMethodCallAsync(Func<Task> method, object parameter, [CallerMemberName] string methodName = "")
        {
            try
            {
                _logger.Enter(TypeName, parameter, methodName);

                var result = method();

                _logger.Exit(TypeName, methodName);

                return result;
            }
            catch (Exception e)
            {
                _logger.Exception(TypeName, e, methodName);
                throw;
            }
        }

        protected Task LogMethodCallAsync(Func<Task> method, [CallerMemberName] string methodName = "", params object[] parameter)
        {
            try
            {
                _logger.Enter(TypeName, parameter, methodName);

                var result = method();

                _logger.Exit(TypeName, methodName);

                return result;
            }
            catch (Exception e)
            {
                _logger.Exception(TypeName, e, methodName);
                throw;
            }
        }
    }
}