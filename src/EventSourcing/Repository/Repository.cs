using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Domain;
using EventSourcing.Event;
using EventSourcing.EventBus;
using EventSourcing.Exceptions;

namespace EventSourcing.Repository
{
    public class Repository : IRepository
    {
        private readonly IEventStorageProvider _eventStorageProvider;
        private readonly ISnapshotStorageProvider _snapshotStorageProvider;
        private readonly IEventPublisher _eventPublisher;

        public Repository(IEventStorageProvider eventStorageProvider, ISnapshotStorageProvider snapshotStorageProvider, IEventPublisher eventPublisher)
        {
            if (eventStorageProvider == null) throw new ArgumentNullException(nameof(eventStorageProvider));
            if (snapshotStorageProvider == null) throw new ArgumentNullException(nameof(snapshotStorageProvider));
            if (eventPublisher == null) throw new ArgumentNullException(nameof(eventPublisher));

            _eventStorageProvider = eventStorageProvider;
            _snapshotStorageProvider = snapshotStorageProvider;
            _eventPublisher = eventPublisher;
        }
        
        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate
        {
            var item = default(TAggregate);

            var isSnapshottable = typeof(ISnapshottable).IsAssignableFrom(typeof(TAggregate));
            Snapshot snapshot = null;

            if ((isSnapshottable) && (_snapshotStorageProvider != null))
            {
                snapshot = await _snapshotStorageProvider.GetSnapshotAsync(typeof(TAggregate), id);
            }

            if (snapshot != null)
            {
                item = ConstructAggregate<TAggregate>();
                ((ISnapshottable)item).ApplySnapshot(snapshot);
                var events = await _eventStorageProvider.GetEventsAsync(typeof(TAggregate), id, snapshot.Version + 1, int.MaxValue);
                item.LoadFromHistory(events);
            }
            else
            {
                var events = (await _eventStorageProvider.GetEventsAsync(typeof(TAggregate), id, 0, int.MaxValue)).ToList();

                if (events.Any())
                {
                    item = ConstructAggregate<TAggregate>();
                    item.LoadFromHistory(events);
                }
            }

            return item;
        }

        public async Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            if (aggregate.HasUncommittedChanges())
            {
                await CommitChanges(aggregate);
            }
        }

        private async Task CommitChanges(Aggregate aggregate)
        {
            var expectedVersion = aggregate.LastCommittedVersion;

            var item = await _eventStorageProvider.GetLastEventAsync(aggregate.GetType(), aggregate.Id);

            if ((item != null) && (expectedVersion == (int)Aggregate.StreamState.NoStream))
            {
                throw new AggregateCreationException(item.CorrelationId, item.TargetVersion +1);
            }
            if ((item != null) && ((item.TargetVersion + 1) != expectedVersion))
            {
                throw new ConcurrencyException(item.CorrelationId);
            }

            var changesToCommit = aggregate.GetUncommittedChanges().ToList();

            //perform pre commit actions
            foreach (var e in changesToCommit)
            {
                DoPreCommitTasks(e);
            }

            //CommitAsync events to storage provider
            await _eventStorageProvider.CommitChangesAsync(aggregate);

            //Publish to event publisher asynchronously
            foreach (var e in changesToCommit)
            {
                await _eventPublisher.PublishAsync(e);
            }

            //If the Aggregate implements snaphottable
            var snapshottable = aggregate as ISnapshottable;

            if ((snapshottable != null))
            {
                //Every N events we save a snapshot
                if (ShouldCreateSnapShot(aggregate, changesToCommit))
                {
                    await _snapshotStorageProvider.SaveSnapshotAsync(aggregate.GetType(), snapshottable.TakeSnapshot());
                }
            }

            aggregate.MarkChangesAsCommitted();
        }

        private bool ShouldCreateSnapShot(Aggregate aggregate, List<IEvent> changesToCommit)
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
            e.EventCommittedTimestamp = DateTime.UtcNow;
        }
        
        private static TAggregate ConstructAggregate<TAggregate>()
        {
            return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true);
        }
    }
}