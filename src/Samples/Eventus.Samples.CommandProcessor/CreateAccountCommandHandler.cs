using System.Threading.Tasks;
using Eventus.Samples.Contracts.BankAccount;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.ReadLayer;
using Eventus.Storage;
using Serilog;

namespace Eventus.Samples.CommandProcessor
{
    public class CreateAccountCommandHandler
    {
        private readonly IRepository _repo;
        private readonly BankAccountReadRepository _readRepository;

        public CreateAccountCommandHandler(IRepository repo, BankAccountReadRepository readRepository)
        {
            _repo = repo;
            _readRepository = readRepository;
        }

        public async Task Handle(CreateAccountCommand command)
        {
            Log.Information("Handling account created command for {aggregate} correlationId:{correlationId}", command.AggregateId, command.CorrelationId);

            var account = new BankAccount(command.AggregateId, command.Name);
            await _repo.SaveAsync(account)
                .ConfigureAwait(false);

            //todo this should really respond to the account created event (implement a custom event publisher this could be in memory or via Rabbit)
            _readRepository.Save(new BankAccountSummary
            {
                Id = command.AggregateId,
                Name = command.Name
            });
        }
    }
}