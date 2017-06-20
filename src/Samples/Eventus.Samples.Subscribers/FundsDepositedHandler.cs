using System.Threading.Tasks;
using Eventus.Samples.Core.Events;
using Eventus.Samples.ReadLayer;

namespace Eventus.Samples.Subscribers
{
    public class FundsDepositedHandler
    {
        private readonly BankAccountReadRepository _readRepository;

        public FundsDepositedHandler(BankAccountReadRepository readRepository)
        {
            _readRepository = readRepository;
        }

        public Task Handle(FundsDepositedEvent @event)
        {
            return Task.Run(() =>
            {
                var summary = _readRepository.Get(@event.AggregateId);

                summary.Balance += @event.Amount;

                _readRepository.Save(summary);
            });
        }
    }
}