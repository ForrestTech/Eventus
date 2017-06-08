using System.Threading.Tasks;
using Eventus.Samples.Core.Events;
using Eventus.Samples.Core.ReadModel;

namespace Eventus.Samples.Core.EventHandlers
{
    public class DepositEventHandler : IHandleEvent<FundsDepositedEvent>
    {
        private readonly IBankAccountReadModelRepository _repository;

        public DepositEventHandler(IBankAccountReadModelRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(FundsDepositedEvent @event)
        {
            var account = await _repository.GetAsync(@event.AggregateId)
                .ConfigureAwait(false) ?? new BankAccountSummary();

            account.Balance += @event.Amount;

            await _repository.SaveAsync(account)
                .ConfigureAwait(false);
        }
    }
}