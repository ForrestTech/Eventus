using System;
using System.Threading.Tasks;
using EventSourcing.Domain;

namespace EventSourcing.Repository
{
    public interface IRepository
    {
        Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate;
        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate;
    }
}