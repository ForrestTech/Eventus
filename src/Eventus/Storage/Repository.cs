using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventus.Domain;
using Eventus.EventBus;
using Eventus.Events;
using Eventus.Exceptions;
using Eventus.Logging;

namespace Eventus.Storage
{
    public class Repository : IRepository
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IEventStorageProvider _eventStorageProvider;
        private readonly ISnapshotStorageProvider _snapshotStorageProvider;
        private readonly IEventPublisher _eventPublisher;

        public Repository(IEventStorageProvider eventStorageProvider, ISnapshotStorageProvider snapshotStorageProvider, IEventPublisher eventPublisher = null)
        {
            _eventStorageProvider = eventStorageProvider ?? throw new ArgumentNullException(nameof(eventStorageProvider));
            _snapshotStorageProvider = snapshotStorageProvider ?? throw new ArgumentNullException(nameof(snapshotStorageProvider));
            _eventPublisher = eventPublisher;
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            var item = default(TAggregate);
            var isSnapshottable = typeof(ISnapshottable).IsAssignableFrom(typeof(TAggregate));
            Snapshot snapshot = null;

            if (isSnapshottable)
            {
                snapshot = await _snapshotStorageProvider.GetSnapshotAsync(typeof(TAggregate), id)
                    .ConfigureAwait(false);
            }

            if (snapshot != null)
            {
                Logger.Debug("Building aggregate from snapshot");

                item = ConstructAggregate<TAggregate>();
                ((ISnapshottable)item).ApplySnapshot(snapshot);

                var events = await _eventStorageProvider.GetEventsAsync(typeof(TAggregate), id, snapshot.Version + 1)
                    .ConfigureAwait(false);
                item.LoadFromHistory(events);
            }
            else
            {
                var events = (await _eventStorageProvider.GetEventsAsync(typeof(TAggregate), id).ConfigureAwait(false)).ToList();

                if (events.Any())
                {
                    item = ConstructAggregate<TAggregate>();
                    item.LoadFromHistory(events);
                }
            }

            return item;
        }

        public Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            if (aggregate.HasUncommittedChanges())
            {
                Logger.DebugFormat("Aggregate has uncommitted changes");

                return CommitChangesAsync(aggregate);
            }
            return Task.CompletedTask;
        }

        private async Task CommitChangesAsync(Aggregate aggregate)
        {
            var expectedVersion = aggregate.LastCommittedVersion;

            var item = await _eventStorageProvider.GetLastEventAsync(aggregate.GetType(), aggregate.Id)
                .ConfigureAwait(false);

            if (item != null && expectedVersion == (int)Aggregate.StreamState.NoStream)
            {
                throw new AggregateCreationException(aggregate.Id, item.TargetVersion + 1);
            }
            if (item != null && item.TargetVersion + 1 != expectedVersion)
            {
                throw new ConcurrencyException(aggregate.Id);
            }

            var changesToCommit = aggregate.GetUncommittedChanges().ToList();

            Logger.Debug("Performing pre commit checks");

            //perform pre commit actions
            foreach (var e in changesToCommit)
            {
                DoPreCommitTasks(e);
            }

            //CommitAsync events to storage provider
            await _eventStorageProvider.CommitChangesAsync(aggregate)
                .ConfigureAwait(false);

            //Publish to event publisher asynchronously
            if (_eventPublisher != null)
            {
                foreach (var e in changesToCommit)
                {
                    await _eventPublisher.PublishAsync(e)
                        .ConfigureAwait(false);
                }
            }

            //If the Aggregate implements snapshottable
            var snapshottable = aggregate as ISnapshottable;

            if (snapshottable != null && _snapshotStorageProvider != null)
            {
                //Every N events we save a snapshot
                if (ShouldCreateSnapShot(aggregate, changesToCommit))
                {
                    await _snapshotStorageProvider.SaveSnapshotAsync(aggregate.GetType(), snapshottable.TakeSnapshot())
                        .ConfigureAwait(false);
                }
            }

            aggregate.MarkChangesAsCommitted();
        }

        private bool ShouldCreateSnapShot(Aggregate aggregate, IReadOnlyCollection<IEvent> changesToCommit)
        {
            return (aggregate.CurrentVersion >= _snapshotStorageProvider.SnapshotFrequency) &&
                   (
                       (changesToCommit.Count >= _snapshotStorageProvider.SnapshotFrequency) ||
                       (aggregate.CurrentVersion % _snapshotStorageProvider.SnapshotFrequency < changesToCommit.Count) ||
                       (aggregate.CurrentVersion % _snapshotStorageProvider.SnapshotFrequency == 0)
                   );
        }

        private static void DoPreCommitTasks(IEvent e)
        {
            e.EventCommittedTimestamp = Clock.Now();
        }

        private static TAggregate ConstructAggregate<TAggregate>()
        {
            return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true);
        }
    }
}