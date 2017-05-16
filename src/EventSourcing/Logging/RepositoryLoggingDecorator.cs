using System;
using System.Threading.Tasks;
using EventSourcing.Domain;
using EventSourcing.Storage;

namespace EventSourcing.Logging
{
    public class RepositoryLoggingDecorator : LoggingDecorator, IRepository
    {
        private readonly IRepository _decorated;

        protected override string TypeName => "Event Sourcing Repository";

        public RepositoryLoggingDecorator(IRepository decorated)
        {
            _decorated = decorated;
        }

        public Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            return LogMethodCallAsync(() => _decorated.SaveAsync(aggregate), aggregate);
        }

        public Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            return LogMethodCallAsync(() => _decorated.GetByIdAsync<TAggregate>(id), id);
        }
    }
}