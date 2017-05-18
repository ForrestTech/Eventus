using System.Threading.Tasks;
using Eventus.Samples.Core.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Storage;

namespace Eventus.Samples.Core.Handlers
{
    public class BankAccountCommandHandlers : IHandleCommands<CreateAccountCommand>,
        IHandleCommands<WithdrawFundsCommand>,
        IHandleCommands<DepostiFundsCommand>
    {
        private readonly IRepository _repository;

        public BankAccountCommandHandlers(IRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(CreateAccountCommand command)
        {
            var account = new BankAccount(command.AggregateId, command.Name);
            await _repository.SaveAsync(account);
        }

        public async Task HandleAsync(WithdrawFundsCommand command)
        {
            var account = await _repository.GetByIdAsync<BankAccount>(command.AggregateId);
            account.WithDrawFunds(command.Amount);

            await _repository.SaveAsync(account);
        }

        public async Task HandleAsync(DepostiFundsCommand command)
        {
            var account = await _repository.GetByIdAsync<BankAccount>(command.AggregateId);
            account.Deposit(command.Amount);

            await _repository.SaveAsync(account);
        }
    }
}