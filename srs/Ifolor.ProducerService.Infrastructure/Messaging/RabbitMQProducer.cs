using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Collections.Concurrent;
using System.Text;

namespace Ifolor.ProducerService.Infrastructure.Messaging
{
    public class RabbitMQProducer : IMessageProducer
    {
        private IConnection _connection;
        private RabbitMQConfig _rabbitMQConfig;
        private ConcurrentBag<IChannel> _channelPool;

        public RabbitMQProducer()
        {
            _channelPool = new ConcurrentBag<IChannel>();
        }

        public async Task SendMessage(RabbitMQConfig rabbitMQConfig, string message)
        {
            var factory = new ConnectionFactory { 
                HostName = rabbitMQConfig.HostName, 
                UserName = rabbitMQConfig.Username, 
                Password = rabbitMQConfig.Password };

            if (_connection == null || !_connection.IsOpen)
            {
                await Connect(rabbitMQConfig);
            }

            if (!_channelPool.TryTake(out var channel))
            {
                channel = await _connection.CreateChannelAsync();
                await channel.QueueDeclareAsync(queue: _rabbitMQConfig.QueueName, durable: false, exclusive: false, autoDelete: false,
                    arguments: null);
            }
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: _rabbitMQConfig.QueueName, body: body);
                Console.WriteLine($" [x] Sent {message}");
            }
            finally
            {
                _channelPool.Add(channel);
            }

            Console.WriteLine($" [x] Sent {message}");
        }

        private async Task Connect(RabbitMQConfig rabbitMQConfig)
        {
            _rabbitMQConfig = rabbitMQConfig;
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQConfig.HostName,
                UserName = _rabbitMQConfig.Username,
                Password = _rabbitMQConfig.Password
            };

            throw new BrokerUnreachableException(new Exception());

            _connection = await factory.CreateConnectionAsync();
        }
    }
}
    