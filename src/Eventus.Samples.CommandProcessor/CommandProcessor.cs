using EasyNetQ;
using Eventus.Samples.Contracts;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Infrastructure.Factories;
using Eventus.Samples.ReadLayer;

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
            //todo move to secret config 
            _bus = RabbitHutch.CreateBus("amqp://hixcoooi:e1cfuLboejhi-Cwm1POIXQ3F3HyA8iyv@puma.rmq.cloudamqp.com/hixcoooi");

            var eventRepo = ProviderFactory.Current.CreateRepositoryAsync().Result;
            var readRepo = new BankAccountReadRepository("Password1@redis-15191.c1.eu-west-1-3.ec2.cloud.redislabs.com:15191");

            _createAccountHandler = new CreateAccountCommandHandler(eventRepo, readRepo);
            _depositHandler = new DepositCommandHandler(eventRepo, readRepo);
            _withDrawHandler = new WithdrawCommandHandler(eventRepo, readRepo);
        }

        public void Start()
        {
            _bus.Receive(Resources.BankAccountQueueName, x => x
                .Add<CreateAccountCommand>(command => _createAccountHandler.Handle(command))
                .Add<DepositFundsCommand>(command => _depositHandler.Handle(command))
                .Add<WithdrawFundsCommand>(command => _withDrawHandler.Handle(command)));
        }

        public void Stop()
        {
        }
    }
}