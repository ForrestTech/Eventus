using System.Linq;
using System.Threading.Tasks;
using EventSourceDemo.Events;
using EventSourceDemo.ReadModel;

namespace EventSourceDemo.EventHandlers
{
    public class WithdrawalEventHandler : IHandleEvent<FundsWithdrawalEvent>
    {
        private readonly IReadModelRepository _repository;

        public WithdrawalEventHandler(IReadModelRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(FundsWithdrawalEvent @event)
        {
            var readmodel = await _repository.GetAsync() ?? new TopAccountsReadModel();

            var account = readmodel.Accounts.SingleOrDefault(x => x.Id == @event.AggregateId);

            if (account == null)
            {
                readmodel.Accounts.Add(new AccountSummary
                {
                    Balance = @event.Amount,
                    Id = @event.AggregateId
                });
            }
            else
            {
                account.Balance -= @event.Amount;
            }

            await _repository.SaveAsync(readmodel);
        }
    }
}