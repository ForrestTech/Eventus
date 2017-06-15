using EasyNetQ;
using Eventus.Samples.Contracts.BankAccount.Commands;
using Eventus.Samples.Core.Domain;
using Eventus.Samples.Infrastructure.Factories;
using Eventus.Storage;
using Topshelf;

namespace Eventus.Samples.CommandProcessor
{
    //todo add serilog and seq to command processor
    //todo test easynetq see if it will work 
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>                                 
            {
                x.Service<CommandProcessor>(s =>
                {
                    s.ConstructUsing(name => new CommandProcessor());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Eventus Command Processor Service");
                x.SetDisplayName("Eventus Command Processor");
                x.SetServiceName("Eventus");
            });
        }
    }

    public class CommandProcessor
    {
        private readonly IBus _bus;
        private readonly CreateAccountHandler _createAccountHandler;

        public CommandProcessor()
        {
            _bus = RabbitHutch.CreateBus("amqp://hixcoooi:e1cfuLboejhi-Cwm1POIXQ3F3HyA8iyv@puma.rmq.cloudamqp.com/hixcoooi");

            var repo = ProviderFactory.Current.CreateRepositoryAsync().Result;
            _createAccountHandler = new CreateAccountHandler(repo);
        }

        public void Start()
        {
            _bus.Receive<CreateAccountCommand>("eventus.account.create", c => _createAccountHandler.Handle(c));
        }

        public void Stop()
        {
        }
    }

    public class CreateAccountHandler
    {
        private readonly IRepository _repo;

        public CreateAccountHandler(IRepository repo)
        {
            _repo = repo;
        }

        public void Handle(CreateAccountCommand command)
        {
            var account = new BankAccount(command.AggregateId, command.Name);
            _repo.SaveAsync(account).Wait();
        }
    }

    /*
    internal class RabbitMQClient
    {
        private readonly ConnectionFactory _factory;

        public RabbitMQClient(string uri)
        {
            //todo move to config 
            _factory = new ConnectionFactory { Uri = uri };
        }

        public void RegisterConsumer<T>(string queueName, Action<T> onMessage)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var item = JsonConvert.DeserializeObject<T>(message);
                        onMessage(item);

                        Console.WriteLine(" [x] Received {0}", message);
                    };

                    channel.BasicConsume(queue: queueName,
                        noAck: true,
                        consumer: consumer);
                }
            }
        }

        private void CreateQueue(IModel channel, string queueName)
        {
            channel.QueueDeclare(queue: "hello",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        private byte[] SerialiseContent(object message)
        {
            var content = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(content);
            return body;
        }
    }
    */
}
