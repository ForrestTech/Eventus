using System.Threading.Tasks;

namespace EventSourceDemo.ReadModel
{
    public interface IReadModelRepository
    {
        Task SaveAsync(TopAccountsReadModel model);
        Task<TopAccountsReadModel> GetAsync();
    }
}