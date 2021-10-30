
namespace Eventus.Storage
{
    using Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using EventBus;
    using Events;
    using Exceptions;
    using Microsoft.Extensions.Logging;
    
    /// <summary>
    /// The default Eventus Aggregate repository
    /// </summary>
    public class Repository : IRepository
    {
        private readonly IEventStorageProvider _eventStorageProvider;
        private readonly ISnapshotStorageProvider _snapshotStorageProvider;
        private readonly EventusOptions _options;
        private readonly IEventPublisher? _eventPublisher;
        private readonly ILogger<Repository> _logger;

        public Repository(IEventStorageProvider eventStorageProvider, 
            ISnapshotStorageProvider snapshotStorageProvider,
            EventusOptions options,
            ILogger<Repository> logger,
            IEventPublisher? eventPublisher = null)
        {
            _eventStorageProvider = eventStorageProvider ?? throw new ArgumentNullException(nameof(eventStorageProvider));
            _snapshotStorageProvider = snapshotStorageProvider ?? throw new ArgumentNullException(nameof(snapshotStorageProvider));
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
                snapshot = await _snapshotStorageProvider.GetSnapshotAsync(typeof(TAggregate), id);
            }

            return snapshot;
        }

        private async Task<TAggregate?> LoadFromSnapshot<TAggregate>(Guid id, Snapshot snapshot) where TAggregate : Aggregate
        {
            _logger.LogDebug("Building aggregate from snapshot");

            var item = ConstructAggregate<TAggregate>();

            if (item == null)
            {
                return null;
            }

            (item as ISnapshottable)?.ApplySnapshot(snapshot);

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
            if (!aggregate.HasUncommittedChanges()) 
                return Task.CompletedTask;
            
            _logger.LogDebug("Aggregate has uncommitted changes");

            return CommitChangesAsync(aggregate);
        }

        private async Task CommitChangesAsync(Aggregate aggregate)
        {
            var expectedVersion = aggregate.LastCommittedVersion;

            var item = await _eventStorageProvider.GetLastEventAsync(aggregate.GetType(), aggregate.Id);

            if (item != null && expectedVersion == (int)Aggregate.StreamState.NoStream)
            {
                throw new AggregateCreationException(aggregate.Id, item.TargetVersion + 1);
            }
            if (item != null && item.TargetVersion + 1 != expectedVersion)
            {
                throw new ConcurrencyException(aggregate.Id);
            }

            var changesToCommit = aggregate.GetUncommittedChanges().ToList();

            _logger.LogDebug("Performing pre commit checks");

            //perform pre commit actions
            foreach (var e in changesToCommit)
            {
                DoPreCommitTasks(e);
            }

            //CommitAsync events to storage provider
            await _eventStorageProvider.CommitChangesAsync(aggregate);

            //Publish to event publisher asynchronously
            if (_eventPublisher != null)
            {
                foreach (var e in changesToCommit)
                {
                    await _eventPublisher.PublishAsync(e);
                }
            }

            //If the Aggregate implements snapshottable
            if (_options.SnapshottingEnabled && aggregate is ISnapshottable snapshottable)
            {
                //Every N events we save a snapshot
                if (ShouldCreateSnapShot(aggregate, changesToCommit))
                {
                    await _snapshotStorageProvider.SaveSnapshotAsync(aggregate.GetType(), snapshottable.TakeSnapshot());
                }
            }

            aggregate.MarkChangesAsCommitted();
        }

        private bool ShouldCreateSnapShot(Aggregate aggregate, IReadOnlyCollection<IEvent> changesToCommit)
        {
            int snapshotFrequency = _options.GetSnapshotFrequency(aggregate.GetType());
            
            return aggregate.CurrentVersion >= snapshotFrequency &&
                   (
                       changesToCommit.Count >= snapshotFrequency ||
                       aggregate.CurrentVersion % snapshotFrequency < changesToCommit.Count ||
                       aggregate.CurrentVersion % snapshotFrequency == 0
                   );
        }

        private static void DoPreCommitTasks(IEvent e)
        {
            e.EventCommittedTimestamp = Clock.Now();
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