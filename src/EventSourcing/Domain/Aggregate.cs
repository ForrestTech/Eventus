﻿using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.Events;
using EventSourcing.Exceptions;

namespace EventSourcing.Domain
{

    public abstract class Aggregate
    {
        public enum StreamState
        {
            NoStream = -1,
            HasStream = 1
        }

        private readonly List<IEvent> _uncommittedChanges;
        private Dictionary<Type, string> _eventHandlerCache;

        protected Aggregate(List<IEvent> uncommittedChanges)
        {
            _uncommittedChanges = uncommittedChanges;
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

        protected Aggregate()
        {
            CurrentVersion = (int)StreamState.NoStream;
            LastCommittedVersion = (int)StreamState.NoStream;
            _uncommittedChanges = new List<IEvent>();
            SetupInternalEventHandlers();
        }

        public IEnumerable<IEvent> GetUncommittedChanges()
        {
            return _uncommittedChanges.ToList();
        }

        public void MarkChangesAsCommitted()
        {
            _uncommittedChanges.Clear();
            LastCommittedVersion = CurrentVersion;
        }

        public bool HasUncommittedChanges()
        {
            return _uncommittedChanges.Any();
        }

        public void LoadFromHistory(IEnumerable<IEvent> history)
        {
            foreach (var e in history)
                ApplyEvent(e, false);

            LastCommittedVersion = CurrentVersion;
        }

        protected internal void ApplyEvent(IEvent @event)
        {
            ApplyEvent(@event, true);
        }

        protected virtual void ApplyEvent(IEvent @event, bool isNew)
        {
            //todo validate events can be applied to this aggregate
            if (_eventHandlerCache.ContainsKey(@event.GetType()))
            {
                var methodName = _eventHandlerCache[@event.GetType()];

                var method = ReflectionHelper.GetMethod(GetType(), methodName, new[] { @event.GetType() });

                if (method != null)
                {
                    method.Invoke(this, new object[] { @event });
                }
                else
                {
                    throw new AggregateEventOnApplyMethodMissingException($"No event Apply method found on {GetType()} for {@event.GetType()}");
                }
            }
            else
            {
                throw new AggregateEventOnApplyMethodMissingException($"No event handler specified for {@event.GetType()} on {GetType()}");
            }

            if (isNew)
            {
                _uncommittedChanges.Add(@event);
            }

            CurrentVersion++;
        }

        private void SetupInternalEventHandlers()
        {
            _eventHandlerCache = ReflectionHelper.FindEventHandlerMethodsInAggregate(GetType());
        }

        #region Comparison
        private sealed class IdEqualityComparer : IEqualityComparer<Aggregate>
        {
            public bool Equals(Aggregate x, Aggregate y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id.Equals(y.Id);
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Aggregate) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        #endregion
    }
}