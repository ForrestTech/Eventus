using System.Threading.Tasks;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.ReadLayer;
using Eventus.Storage;
using Serilog;

namespace Eventus.Samples.CommandProcessor
{
    public class WithdrawCommandHandler
    {
        private readonly IRepository _repo;
        private readonly BankAccountReadRepository _readRepository;

        public WithdrawCommandHandler(IRepository repo, BankAccountReadRepository readRepository)
        {
            _repo = repo;
            _readRepository = readRepository;
        }

        public async Task Handle(WithdrawFundsCommand command)
        {
            Log.Information("Handling withdrawal command for {aggregate} correlationId:{correlationId}", command.AggregateId, command.CorrelationId);

            var account = await _repo.GetByIdAsync<BankAccount>(command.AggregateId)
                .ConfigureAwait(false);

            account.Withdraw(command.Amount, command.CorrelationId);

            await _repo.SaveAsync(account)
                .ConfigureAwait(false);

            var summary = _readRepository.Get(account.Id);

            summary.Balance -= command.Amount;

            _readRepository.Save(summary);
        }
    }
}