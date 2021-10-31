namespace Eventus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Events;
    using Exceptions;
    using MassTransit;
    using System.Reflection;

    /// <summary>
    /// Common base class for all Eventus, event sourced aggregates
    /// </summary>
    public abstract class Aggregate
    {
        public const int NoEventsStoredYet = 0;

        private readonly List<IEvent> _uncommittedChanges = new();
        private Dictionary<Type, MethodInfo> _eventHandlerCache = new();

        protected Aggregate() : this(NewId.NextGuid())
        {
        }

        protected Aggregate(Guid id)
        {
            Id = id;
            CurrentVersion = NoEventsStoredYet;
            LastCommittedVersion = NoEventsStoredYet;
            SetupInternalEventHandlers();
        }

        /// <summary>
        /// Aggregates unique Guid
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Current version of the Aggregate. Starts with -1 and parameterized constructor increments it by 1.
        /// All events will increment this by 1 when Applied.
        /// </summary>
        public int CurrentVersion { get; protected set; }

        /// <summary>
        /// This is the CurrentVersion of the Aggregate when it was saved last. This is used to ensure optimistic concurrency. 
        /// </summary>
        public int LastCommittedVersion { get; protected set; }

        /// <summary>
        /// Gets a list of uncommitted events for this aggregate.
        /// </summary>
        /// <returns>List of uncommitted changes</returns>
        public List<IEvent> GetUncommittedChanges()
        {
            return _uncommittedChanges.ToList();
        }

        /// <summary>
        /// Mark all changes as committed, clears uncommitted changes and updates the current version of the aggregate
        /// </summary>
        public void MarkChangesAsCommitted()
        {
            _uncommittedChanges.Clear();
            LastCommittedVersion = CurrentVersion;
        }

        /// <summary>
        /// Does the aggregate have change that have not been committed to storage
        /// </summary>
        /// <returns>If the aggregate has uncommitted change</returns>
        public bool HasUncommittedChanges()
        {
            return _uncommittedChanges.Any();
        }

        /// <summary>
        /// Loads the current state of the aggregate from a list of events
        /// </summary>
        /// <param name="history">History of events to apply to the aggregate</param>
        public void LoadFromHistory(IEnumerable<IEvent> history)
        {
            foreach (var e in history)
                ApplyEvent(e, false);

            LastCommittedVersion = CurrentVersion;
        }

        protected virtual void ApplyEvent(IEvent @event, bool isNew = true)
        {
            if (isNew)
            {
                @event.AggregateId = Id;
            }

            if (_eventHandlerCache.ContainsKey(@event.GetType()))
            {
                var method = _eventHandlerCache[@event.GetType()];

                method.Invoke(this, new object[] {@event});
            }
            else
            {
                throw new AggregateEventOnApplyMethodMissingException(
                    $"No event handler specified for {@event.GetType()} on {GetType()}");
            }

            if (isNew)
            {
                _uncommittedChanges.Add(@event);
            }

            CurrentVersion++;
        }

        private void SetupInternalEventHandlers()
        {
            _eventHandlerCache = AggregateCache.GetEventHandlersForAggregate(GetType());
        }

        #region Comparison

        private sealed class IdEqualityComparer : IEqualityComparer<Aggregate>
        {
            public bool Equals(Aggregate? x, Aggregate? y)
            {
                if (ReferenceEquals(null, y)) return false;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(x, y)) return true;
                return x.GetType() == y.GetType() && x.Id.Equals(y.Id);
            }

            public int GetHashCode(Aggregate obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        public static IEqualityComparer<Aggregate> IdComparer { get; } = new IdEqualityComparer();

        protected bool Equals(Aggregate other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Aggregate)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}