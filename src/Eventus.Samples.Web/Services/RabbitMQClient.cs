using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Eventus.Samples.Web.Services
{
    public class RabbitMQClient
    {
        private readonly ConnectionFactory _factory;

        public RabbitMQClient(string uri)
        {
            _factory = new ConnectionFactory { Uri = uri };
        }

        public void Send(string queueName, object message)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    CreateQueue(channel, queueName);
                    var body = SerialiseContent(message);

                    var properties = channel.CreateBasicProperties();
                    var type = message.GetType();

                    properties.Type = type.FullName + ":" + type.GetTypeInfo().Assembly.GetName().Name;

                    channel.BasicPublish(exchange: "",
                        routingKey: queueName,
                        basicProperties: properties,
                        body: body);
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
}