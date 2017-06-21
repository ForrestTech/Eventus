using System.Threading.Tasks;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Storage;
using Serilog;

namespace Eventus.Samples.CommandProcessor
{
    public class DepositCommandHandler
    {
        private readonly IRepository _repo;

        public DepositCommandHandler(IRepository repo)
        {
            _repo = repo;
        }

        public async Task Handle(DepositFundsCommand command)
        {
            Log.Information("Handling deposit command for {aggregate} correlationId:{correlationId}", command.AggregateId, command.CorrelationId);

            var account = await _repo.GetByIdAsync<BankAccount>(command.AggregateId)
                .ConfigureAwait(false);

            account.Deposit(command.Amount, command.CorrelationId);

            await _repo.SaveAsync(account)
                .ConfigureAwait(false);
        }
    }
}