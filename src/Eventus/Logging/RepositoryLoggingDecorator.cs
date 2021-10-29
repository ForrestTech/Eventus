namespace Eventus.Logging
{
    using System;
    using System.Threading.Tasks;
    using Domain;
    using Storage;
    using Microsoft.Extensions.Logging;
    
    public class RepositoryLoggingDecorator : LoggingDecorator, IRepository
    {
        private readonly IRepository _decorated;

        protected override string TypeName => "Event Sourcing Repository";

        public RepositoryLoggingDecorator(ILogger<RepositoryLoggingDecorator> logger, IRepository decorated): base(logger)
        {
            _decorated = decorated;
        }

        public Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            return LogMethodCallAsync(() => _decorated.SaveAsync(aggregate), aggregate);
        }

        public Task<TAggregate?> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            return LogMethodCallAsync(() => _decorated.GetByIdAsync<TAggregate>(id), id);
        }
    }
}