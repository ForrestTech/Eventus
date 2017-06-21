using System.Configuration;
using EasyNetQ;
using Eventus.Samples.Core.Events;
using Eventus.Samples.ReadLayer;
using Serilog;

namespace Eventus.Samples.Subscribers
{
    public class Subscribers
    {
        private readonly IBus _bus;
        private readonly AccountCreatedHandler _accountCreatedHandler;
        private readonly FundsDepositedHandler _depositHandler;
        private readonly FundsWithdrawalHandler _withDrawHandler;

        public Subscribers()
        {
            _bus = RabbitHutch.CreateBus(ConfigurationManager.AppSettings["RabbitMQUri"]);
            var readRepo = new BankAccountReadRepository(ConfigurationManager.AppSettings["RedisConnectionString"]);

            _accountCreatedHandler = new AccountCreatedHandler(readRepo);
            _depositHandler = new FundsDepositedHandler(readRepo);
            _withDrawHandler = new FundsWithdrawalHandler(readRepo);
        }

        public void Start()
        {
            Log.Information("Subscriber Started");

            var subscriptionId = "Subscriber";
            
            _bus.Subscribe<AccountCreatedEvent>(subscriptionId, @event => _accountCreatedHandler.Handle(@event));
            _bus.Subscribe<FundsDepositedEvent>(subscriptionId, @event => _depositHandler.Handle(@event));
            _bus.Subscribe<FundsWithdrawalEvent>(subscriptionId, @event => _withDrawHandler.Handle(@event));
        }

        public void Stop()
        {
            Log.Information("Subscriber Stopped");
        }
    }
}