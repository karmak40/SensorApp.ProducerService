using Ifolor.ProducerService.Core.Services;
using IfolorProducerService.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Ifolor.ProducerService.Infrastructure.Messaging
{
    public class RabbitMQProducer : IDisposable, IMessageProducer
    {
        private IConnection _connection;
        private readonly ILogger<RabbitMQProducer> _logger;
        private RabbitMQConfig _rabbitMQConfig;
        private ConcurrentBag<IChannel> _channelPool;

        public RabbitMQProducer(IOptions<RabbitMQConfig> rabbitMQConfig, ILogger<RabbitMQProducer> logger)
        {
            _channelPool = new ConcurrentBag<IChannel>();
            _logger = logger;
            _rabbitMQConfig = rabbitMQConfig.Value;
        }

        public async Task SendMessage(SensorData sensorData)
        {
            var message = JsonSerializer.Serialize(sensorData);
            var factory = new ConnectionFactory { 
                HostName = _rabbitMQConfig.HostName, 
                UserName = _rabbitMQConfig.Username, 
                Password = _rabbitMQConfig.Password };

            if (_connection == null || !_connection.IsOpen)
            {
                await Connect(_rabbitMQConfig);
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

            _connection = await factory.CreateConnectionAsync();
        }

        public void Dispose()
        {
            try
            {
                _connection.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
            finally
            {
                _connection.Dispose();
            }
        }
    }
}
    