using System.Threading.Tasks;
using Eventus.Samples.Core.Events;
using Eventus.Samples.ReadLayer;

namespace Eventus.Samples.Subscribers
{
    public class FundsWithdrawalHandler
    {
        private readonly BankAccountReadRepository _readRepository;

        public FundsWithdrawalHandler(BankAccountReadRepository readRepository)
        {
            _readRepository = readRepository;
        }

        public Task Handle(FundsWithdrawalEvent @event)
        {
            return Task.Run(() =>
            {
                var summary = _readRepository.Get(@event.AggregateId);

                summary.Balance -= @event.Amount;

                _readRepository.Save(summary);
            });
        }
    }
}