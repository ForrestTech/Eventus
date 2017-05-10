using System.Threading.Tasks;

namespace EventSourcing.Samples.Core.ReadModel
{
    public interface IReadModelRepository
    {
        Task SaveAsync(TopAccountsReadModel model);
        Task<TopAccountsReadModel> GetAsync();
    }
}