using System.Threading.Tasks;
using Eventus.Samples.Core.Events;
using Eventus.Samples.Core.ReadModel;

namespace Eventus.Samples.Core.EventHandlers
{
    public class AccountCreatedEventHandler : IHandleEvent<AccountCreatedEvent>
    {
        private readonly IBankAccountReadModelRepository _repository;

        public AccountCreatedEventHandler(IBankAccountReadModelRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(AccountCreatedEvent @event)
        {
            var summary = new BankAccountSummary
            {
                Id = @event.AggregateId,
                Name = @event.Name
            };

            return _repository.SaveAsync(summary);
        }
    }
}