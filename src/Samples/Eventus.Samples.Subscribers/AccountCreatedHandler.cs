using System.Threading.Tasks;
using Eventus.Samples.Contracts.BankAccount;
using Eventus.Samples.Core.Events;
using Eventus.Samples.ReadLayer;

namespace Eventus.Samples.Subscribers
{
    public class AccountCreatedHandler
    {
        private readonly BankAccountReadRepository _readRepository;

        public AccountCreatedHandler(BankAccountReadRepository readRepository)
        {
            _readRepository = readRepository;
        }

        public Task Handle(AccountCreatedEvent @event)
        {
            return Task.Run(() =>
            {
                _readRepository.Save(new BankAccountSummary
                {
                    Id = @event.AggregateId,
                    Name = @event.Name
                });
            });
        }
    }
}