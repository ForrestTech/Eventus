using System;
using System.Threading.Tasks;

namespace Eventus.Samples.Core.ReadModel
{
    public interface IBankAccountReadModelRepository
    {
        Task SaveAsync(BankAccountSummary account);

        Task<BankAccountSummary> GetAsync(Guid id);
    }
}