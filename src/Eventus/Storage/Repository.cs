namespace Eventus.Storage
{
    using Configuration;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using EventBus;
    using Exceptions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The default Eventus Aggregate repository
    /// </summary>
    public class Repository : IRepository
    {
        private readonly IEventStorageProvider _eventStorageProvider;
        private readonly ISnapshotStorageProvider _snapshotStorageProvider;
        private readonly ISnapshotCalculator _snapshotCalculator;
        private readonly EventusOptions _options;
        private readonly IEventPublisher? _eventPublisher;
        private readonly ILogger<Repository> _logger;

        public Repository(IEventStorageProvider eventStorageProvider,
            ISnapshotStorageProvider snapshotStorageProvider,
            ISnapshotCalculator snapshotCalculator,
            EventusOptions options,
            ILogger<Repository> logger,
            IEventPublisher? eventPublisher = null)
        {
            _eventStorageProvider = eventStorageProvider;
            _snapshotStorageProvider = snapshotStorageProvider;
            _snapshotCalculator = snapshotCalculator;
            _options = options;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<TAggregate?> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            var snapshot = await GetPossibleSnapshot<TAggregate>(id);

            if (snapshot != null)
            {
                return await LoadFromSnapshot<TAggregate>(id, snapshot);
            }

            return await LoadFromEvents<TAggregate>(id);
        }

        private async Task<Snapshot?> GetPossibleSnapshot<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            var isSnapshottable = typeof(ISnapshottable).IsAssignableFrom(typeof(TAggregate));
            Snapshot? snapshot = null;

            if (_options.SnapshottingEnabled && isSnapshottable)
            {
                _logger.LogDebug(
                    "Aggregate: '{Aggregate}' is marked as snapshottable and Snapshotting is enabled so trying to find the most recent snapshot for aggregate",
                    id);
                snapshot = await _snapshotStorageProvider.GetSnapshotAsync(typeof(TAggregate), id);
            }

            return snapshot;
        }

        private async Task<TAggregate?> LoadFromSnapshot<TAggregate>(Guid id, Snapshot snapshot)
            where TAggregate : Aggregate
        {
            _logger.LogDebug("Building aggregate: '{Aggregate}' from snapshot", id);

            var item = ConstructAggregate<TAggregate>();

            if (item == null)
            {
                return null;
            }

            (item as ISnapshottable)?.ApplySnapshot(snapshot);

            _logger.LogDebug(
                "Loading events for aggregate: '{Aggregate}' since last snapshot at version: {SnapshotVersion}", id,
                snapshot.Version);

            var events = await _eventStorageProvider.GetEventsAsync(typeof(TAggregate), id, snapshot.Version + 1);
            var eventList = events.ToList();

            if (eventList.Any())
            {
                item.LoadFromHistory(eventList);
            }

            return item;
        }

        private async Task<TAggregate?> LoadFromEvents<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            _logger.LogDebug("Building aggregate: '{Aggregate}' from events stream", id);

            var events = (await _eventStorageProvider.GetEventsAsync(typeof(TAggregate), id)).ToList();

            if (events.Any())
            {
                var item = ConstructAggregate<TAggregate>();

                if (item == null)
                {
                    return null;
                }

                item.LoadFromHistory(events);

                return item;
            }

            return null;
        }

        public Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            _logger.LogDebug("Saving aggregate: '{Aggregate}'", aggregate.Id);

            if (!aggregate.HasUncommittedChanges())
                return Task.CompletedTask;

            _logger.LogDebug("Aggregate: '{Aggregate}' has '{ChangesToCommit}' uncommitted changes", aggregate.Id, aggregate.GetUncommittedChanges().Count);

            return CommitChangesAsync(aggregate);
        }

        private async Task CommitChangesAsync(Aggregate aggregate)
        {
            await ValidateCanCommitChanges(aggregate);

            var changesToCommit = aggregate.GetUncommittedChanges();
            
            _logger.LogDebug("Committing changes for aggregate: '{Aggregate}'", aggregate.Id);

            await _eventStorageProvider.CommitChangesAsync(aggregate);

            if (_eventPublisher != null)
            {
                _logger.LogDebug("Publishing aggregate: '{Aggregate}' events", aggregate.Id);

                foreach (var e in changesToCommit)
                {
                    await _eventPublisher.PublishAsync(e);
                }
            }

            if (_options.GetSnapshotEnabled(aggregate.GetType()) && aggregate is ISnapshottable snapshottable &&
                _snapshotCalculator.ShouldCreateSnapShot(aggregate))
            {
                _logger.LogDebug(
                    "Taking a snapshot for aggregate: '{Aggregate}' current version: '{CurrentVersion}' changes to commit: {ChangesToCommit}",
                    aggregate.Id, aggregate.CurrentVersion, changesToCommit.Count);
                await _snapshotStorageProvider.SaveSnapshotAsync(aggregate.GetType(), snapshottable.TakeSnapshot());
            }

            _logger.LogDebug("Marking all aggregate: '{Aggregate}' changes as committed", aggregate.Id);

            aggregate.MarkChangesAsCommitted();
        }

        private async Task ValidateCanCommitChanges(Aggregate aggregate)
        {
            var expectedVersion = aggregate.LastCommittedVersion;

            _logger.LogDebug(
                "Getting latest event for aggregate: '{Aggregate}' to validate that aggregate is in a state to be saved",
                aggregate.Id);

            var item = await _eventStorageProvider.GetLastEventAsync(aggregate.GetType(), aggregate.Id);

            if (item != null && expectedVersion == Aggregate.NoEventsStoredYet)
            {
                throw new AggregateCreationException(aggregate.Id, item.TargetVersion + 1);
            }

            if (item != null && item.TargetVersion + 1 != expectedVersion)
            {
                throw new ConcurrencyException(aggregate.Id);
            }
        }

        private static TAggregate? ConstructAggregate<TAggregate>() where TAggregate : Aggregate
        {
            var instance = Activator.CreateInstance(typeof(TAggregate), true);

            if (instance == null)
            {
                return null;
            }

            var aggregate = (TAggregate)instance;

            return aggregate;
        }
    }
}
