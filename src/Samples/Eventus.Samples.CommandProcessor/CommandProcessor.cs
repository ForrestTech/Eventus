using System.Configuration;
using EasyNetQ;
using Eventus.Samples.Contracts;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Infrastructure.Factories;
using Eventus.Samples.ReadLayer;
using Serilog;

namespace Eventus.Samples.CommandProcessor
{
    public class CommandProcessor
    {
        private readonly IBus _bus;
        private readonly CreateAccountCommandHandler _createAccountHandler;
        private readonly DepositCommandHandler _depositHandler;
        private readonly WithdrawCommandHandler _withDrawHandler;

        public CommandProcessor()
        {
            _bus = RabbitHutch.CreateBus(ConfigurationManager.AppSettings["RabbitMQUri"]);

            var eventRepo = ProviderFactory.Current.CreateRepositoryAsync().Result;
            var readRepo = new BankAccountReadRepository(ConfigurationManager.AppSettings["RedisConnectionString"]);

            _createAccountHandler = new CreateAccountCommandHandler(eventRepo, readRepo);
            _depositHandler = new DepositCommandHandler(eventRepo, readRepo);
            _withDrawHandler = new WithdrawCommandHandler(eventRepo, readRepo);
        }

        public void Start()
        {
            Log.Information("Command Processor Started");

            _bus.Receive(Resources.BankAccountQueueName, x => x
                .Add<CreateAccountCommand>(command => _createAccountHandler.Handle(command))
                .Add<DepositFundsCommand>(command => _depositHandler.Handle(command))
                .Add<WithdrawFundsCommand>(command => _withDrawHandler.Handle(command)));
        }

        public void Stop()
        {
            Log.Information("Command Processor Stopped");
        }
    }
}