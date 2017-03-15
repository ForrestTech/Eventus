using System.Threading.Tasks;

namespace EventSourceDemo.ReadModel
{
    public interface IReadModelRepository
    {
        Task Save(TopAccountsReadModel model);
        Task<TopAccountsReadModel> Get();
    }
}