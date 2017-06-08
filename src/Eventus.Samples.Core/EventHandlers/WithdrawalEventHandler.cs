using System.Linq;
using System.Threading.Tasks;
using Eventus.Samples.Core.Events;
using Eventus.Samples.Core.ReadModel;

namespace Eventus.Samples.Core.EventHandlers
{
    public class WithdrawalEventHandler : IHandleEvent<FundsWithdrawalEvent>
    {
        private readonly IBankAccountReadModelRepository _repository;

        public WithdrawalEventHandler(IBankAccountReadModelRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(FundsWithdrawalEvent @event)
        {
            var account = await _repository.GetAsync(@event.AggregateId)
                .ConfigureAwait(false) ?? new BankAccountSummary();

            account.Balance -= @event.Amount;

            await _repository.SaveAsync(account)
                .ConfigureAwait(false);
        }
    }
}