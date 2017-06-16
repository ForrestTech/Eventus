using System.Threading.Tasks;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Storage;

namespace Eventus.Samples.Core.Handlers
{
    public class BankAccountCommandHandlers 
    {
        private readonly IRepository _repository;

        public BankAccountCommandHandlers(IRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(CreateAccountCommand message)
        {
            var account = new BankAccount(message.AggregateId, message.Name, message.CorrelationId);
            return _repository.SaveAsync(account);
        }

        public async Task Handle(WithdrawFundsCommand message)
        {
            var account = await _repository.GetByIdAsync<BankAccount>(message.AggregateId)
                .ConfigureAwait(false);

            account.Withdraw(message.Amount, message.CorrelationId);

            await _repository.SaveAsync(account)
                .ConfigureAwait(false);
        }

        public async Task Handle(DepositFundsCommand message)
        {
            var account = await _repository.GetByIdAsync<BankAccount>(message.AggregateId)
                .ConfigureAwait(false);

            account.Deposit(message.Amount, message.CorrelationId);

            await _repository.SaveAsync(account)
                .ConfigureAwait(false);
        }
    }
}