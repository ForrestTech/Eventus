namespace Eventus.Storage
{
    using System;
    using System.Threading.Tasks;
    using Domain;

    /// <summary>
    /// Eventus Aggregate event storage abstraction
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Save the current aggregate to the underlying storage provider
        /// </summary>
        /// <param name="aggregate">The aggregate to save</param>
        /// <typeparam name="TAggregate">The type of aggregate to save</typeparam>
        Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate;
        
        /// <summary>
        /// Get an aggregate by its id
        /// </summary>
        /// <param name="id">The id of the aggregate to retrieve</param>
        /// <typeparam name="TAggregate">The type of the aggregate</typeparam>
        /// <returns>The aggregate requested or null if non exists</returns>
        Task<TAggregate?> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate;
    }
}