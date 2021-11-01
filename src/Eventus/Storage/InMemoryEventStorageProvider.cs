namespace Eventus.Storage
{
    using Domain;
    using Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// A simple in memory storage provider for events that can be used for development and testing
    /// </summary>
    public class InMemoryEventStorageProvider : IEventStorageProvider
    {
        public static readonly Dictionary<Guid, List<IEvent>> Storage = new();

        public Task<IEnumerable<IEvent>> GetEventsAsync(Type aggregateType, Guid aggregateId, int offSet, int count)
        {
            var result = new List<IEvent>();
            
            if (Storage.TryGetValue(aggregateId, out List<IEvent>? events))
            {
                return Task.FromResult(events.Skip(offSet).Take(count).AsEnumerable());
            }

            if (events == null || !events.Any())
            {
                return Task.FromResult(result.AsEnumerable());
            }
            
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<IEvent?> GetLastEventAsync(Type aggregateType, Guid aggregateId)
        {
            if (Storage.TryGetValue(aggregateId, out List<IEvent>? events))
            {
                var eventList = events.ToList();
                
                if (!eventList.Any())
                {
                    return Task.FromResult<IEvent?>(null);
                }

                var lastEvent = eventList.OrderByDescending(x => x.EventCommittedTimestamp)
                    .Take(1)
                    .Single();

                return Task.FromResult(lastEvent)!;
            }
            
            return Task.FromResult<IEvent?>(null);
        }

        public Task CommitChangesAsync(Aggregate aggregate)
        {
            var events = aggregate.GetUncommittedChanges()
                .ToList();

            if (!events.Any())
            {
                return Task.CompletedTask;
            }

            var key = aggregate.Id;

            if (Storage.TryGetValue(key, out List<IEvent>? existingEvents))
            {
                var list = existingEvents.ToList();

                list.AddRange(events);

                Storage[key] = list;
            }
            else
            {
                Storage[key] = events;
            }

            return Task.CompletedTask;
        }
    }
}