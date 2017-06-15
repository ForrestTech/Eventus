using System.Threading.Tasks;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.ReadLayer;
using Eventus.Storage;

namespace Eventus.Samples.CommandProcessor
{
    public class DepositCommandHandler
    {
        private readonly IRepository _repo;
        private readonly BankAccountReadRepository _readRepository;

        public DepositCommandHandler(IRepository repo, BankAccountReadRepository readRepository)
        {
            _repo = repo;
            _readRepository = readRepository;
        }

        public async Task Handle(DepositFundsCommand command)
        {
            var account = await _repo.GetByIdAsync<BankAccount>(command.AggregateId)
                .ConfigureAwait(false);

            account.Deposit(command.Amount, command.CorrelationId);

            await _repo.SaveAsync(account)
                .ConfigureAwait(false);

            var summary = _readRepository.Get(account.Id);

            summary.Balance += command.Amount;

            _readRepository.Save(summary);
        }
    }
}