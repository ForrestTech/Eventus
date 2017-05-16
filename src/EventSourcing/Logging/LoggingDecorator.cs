using System;
using System.Threading.Tasks;
using EventSourcing.Storage;

namespace EventSourcing.Logging
{
    public abstract class LoggingDecorator<T>
    {
        private static readonly ILog Logger = LogProvider.For<T>();

        protected abstract string TypeName { get; }

        protected async Task<TResult> LogMethodCallAsync<TResult>(Func<Task<TResult>> method, object parameter)
        {
            try
            {
                Logger.Enter(TypeName, parameter);

                var result = await method()
                    .ConfigureAwait(false);

                Logger.Exit(TypeName, result);

                return result;
            }
            catch (Exception e)
            {
                Logger.Exception(TypeName, e);
                throw;
            }
        }

        protected Task LogMethodCallAsync(Func<Task> method, object parameter)
        {
            try
            {
                Logger.Enter(TypeName, parameter);

                var result = method();

                Logger.Exit(TypeName);

                return result;
            }
            catch (Exception e)
            {
                Logger.Exception(TypeName, e);
                throw;
            }
        }
    }
}