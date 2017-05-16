using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EventSourcing.Logging
{
    public abstract class LoggingDecorator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        protected abstract string TypeName { get; }

        protected async Task<TResult> LogMethodCallAsync<TResult>(Func<Task<TResult>> method, object parameter, [CallerMemberName] string methodName = "")
        {
            try
            {
                Logger.Enter(TypeName, parameter, methodName);

                var result = await method()
                    .ConfigureAwait(false);

                Logger.Exit(TypeName, result, methodName);

                return result;
            }
            catch (Exception e)
            {
                Logger.Exception(TypeName, e, methodName);
                throw;
            }
        }

        protected async Task<TResult> LogMethodCallAsync<TResult>(Func<Task<TResult>> method, object[] parameter, [CallerMemberName] string methodName = "")
        {
            try
            {
                Logger.Enter(TypeName, parameter, methodName);

                var result = await method()
                    .ConfigureAwait(false);

                Logger.Exit(TypeName, result, methodName);

                return result;
            }
            catch (Exception e)
            {
                Logger.Exception(TypeName, e);
                throw;
            }
        }

        protected Task LogMethodCallAsync(Func<Task> method, object parameter, [CallerMemberName] string methodName = "")
        {
            try
            {
                Logger.Enter(TypeName, parameter, methodName);

                var result = method();

                Logger.Exit(TypeName, methodName);

                return result;
            }
            catch (Exception e)
            {
                Logger.Exception(TypeName, e, methodName);
                throw;
            }
        }

        protected Task LogMethodCallAsync(Func<Task> method, [CallerMemberName] string methodName = "", params object[] parameter)
        {
            try
            {
                Logger.Enter(TypeName, parameter, methodName);

                var result = method();

                Logger.Exit(TypeName, methodName);

                return result;
            }
            catch (Exception e)
            {
                Logger.Exception(TypeName, e, methodName);
                throw;
            }
        }
    }
}