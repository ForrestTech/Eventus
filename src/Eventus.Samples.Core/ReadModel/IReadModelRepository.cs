using System.Threading.Tasks;

namespace Eventus.Samples.Core.ReadModel
{
    public interface IReadModelRepository
    {
        Task SaveAsync(TopAccountsReadModel model);
        Task<TopAccountsReadModel> GetAsync();
    }
}