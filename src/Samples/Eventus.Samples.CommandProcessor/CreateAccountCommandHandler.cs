using System.Threading.Tasks;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Storage;
using Serilog;

namespace Eventus.Samples.CommandProcessor
{
    public class CreateAccountCommandHandler
    {
        private readonly IRepository _repo;

        public CreateAccountCommandHandler(IRepository repo)
        {
            _repo = repo;
        }

        public Task Handle(CreateAccountCommand command)
        {
            Log.Information("Handling account created command for {aggregate} correlationId:{correlationId}", command.AggregateId, command.CorrelationId);

            var account = new BankAccount(command.AggregateId, command.Name);
            return _repo.SaveAsync(account);
        }
    }
}