﻿using Eventus.Configuration;

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

        private const string TypeName = "Event Sourcing Repository";

        public RepositoryLoggingDecorator(IRepository decorated,
            ILogger<RepositoryLoggingDecorator> logger,
            EventusOptions options) :
            base(logger, options)
        {
            _decorated = decorated;
        }

        public Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            return LogMethodCallAsync(TypeName, () => _decorated.SaveAsync(aggregate), aggregate);
        }

        public Task<TAggregate?> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            return LogMethodCallAsync(TypeName, () => _decorated.GetByIdAsync<TAggregate>(id), id);
        }
    }
}
